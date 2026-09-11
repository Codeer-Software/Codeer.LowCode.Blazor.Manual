using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Codeer.LowCode.Blazor.Utils;
using Microsoft.AspNetCore.Authorization;
using LowCodeNativeSamples.Client;
using LowCodeNativeSamples.Server.Services;
using Codeer.LowCode.Blazor.Extras.Server.Auth;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Microsoft.Extensions.Caching.Distributed;

namespace LowCodeNativeSamples.Server.Controllers
{
    [ApiController, AutoValidateAntiforgeryToken]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        const string LoginPage = "/login.html";

        readonly DataService _dataService;
        readonly ExternalLoginService _externalLogins;
        readonly IDistributedCache _cache;

        public AccountController(DataService dataService, ExternalLoginService externalLogins, IDistributedCache cache)
        {
            _dataService = dataService;
            _externalLogins = externalLogins;
            _cache = cache;
        }

        [Authorize]
        [HttpGet("current_user")]
        public StringWrapper GetCurrentUser()
            => new(DataService.GetCurrentUserId(HttpContext));

        //Sole issuer of the antiforgery token cookie. login.html fetches this before
        //every login attempt, and the WASM client fetches it at startup.
        [HttpGet("antiforgery")]
        public IActionResult Antiforgery()
        {
            CookieAuthentication.AppendAntiforgeryTokenCookie(HttpContext);
            return NoContent();
        }

        //ログイン画面が描くもの: ID/パスワードのフォームの有無、外部 IdP ごとのボタン (appsettings の EntraLogin / GoogleLogin / CognitoLogin / OidcLogins)。
        //見た目は login.html (Web) と Login.razor (MAUI) を直接書き換える
        [HttpGet("login_options")]
        public object LoginOptions()
            => new
            {
                Password = SystemConfig.Instance.AllowPasswordLogin,
                Providers = _externalLogins.Options,
            };

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginInfo? loginInfo)
        {
            if (loginInfo == null) throw new ArgumentException(nameof(loginInfo));
            if (!SystemConfig.Instance.AllowPasswordLogin) return NotFound();

            //ID/パスワードの照合。表・列はユーザーモジュールのデザイン (IdField / LoginAccountContractField の役割 / PasswordHashField のハッシュ・ソルト) から引く
            var designData = DesignerService.GetDesignData();
            var accounts = LoginAccountStore.Create(designData, _dataService.DbAccess);
            if (accounts == null || !accounts.HasPassword) return NotFound();

            var account = await accounts.VerifyPasswordAsync(loginInfo.Id, loginInfo.Password);
            if (account == null) return Unauthorized();

            //二要素認証。コード未指定なら状態を返すだけでサインインしない。コード検証が通ったとき (status: ok) だけ下のサインインへ進む。
            // - 認証アプリ (TOTP): LoginAccountContractField に TOTP の 3 列があるとき (setup = 登録用 QR / totp = コード要求)
            // - メールのワンタイムコード: LoginAccountContractField の TwoFactorEmail があるとき (email = 送信済み)。TOTP の列があればそちらが優先
            var totp = TotpLogin.Create(designData, SystemConfig.Instance.TotpLogin, _dataService.DbAccess);
            if (totp != null)
            {
                var result = await totp.VerifyAsync(account.UserId, account.LoginName, loginInfo.TwoFactorCode);
                if (result.Status != TotpLoginStatus.Ok) return Ok(result);
            }
            else if (accounts.HasTwoFactorEmail)
            {
                //メールは MailDispatcher 経由 (送信インフラの解決・開発環境の宛先リダイレクト)。履歴モジュールには残さない (コードを記録しない)
                var dispatcher = new MailDispatcher(SystemConfig.Instance.Mail, MailSenderTable.Create);
                var email = new EmailOtpLogin(SystemConfig.Instance.EmailOtpLogin,
                    message => dispatcher.SendAsync(new Codeer.LowCode.Blazor.Extras.Mail.MailSendRequest { MailInfraName = SystemConfig.Instance.EmailOtpLogin.MailInfraName, Message = message }), _cache);
                var result = await email.VerifyAsync(account.UserId, account.TwoFactorEmail ?? string.Empty, loginInfo.TwoFactorCode);
                if (result.Status != EmailOtpLoginStatus.Ok) return Ok(result);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, account.DisplayName),
                new(ClaimTypes.NameIdentifier, account.UserId)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = loginInfo.IsPersistent });

            return Ok(new TotpLoginResult { Status = TotpLoginStatus.Ok });
        }

        //外部 IdP: ブラウザがここに遷移 (GET) すると IdP へ送られる。IdP が本人確認した後、ExternalLoginUserResolver が
        //ユーザー行に解決し、パスワードログインと同じ Cookie を発行する。
        //persistent=true は「ログイン状態を保持する」(ブラウザを閉じても残る Cookie)。
        //mobile=true はネイティブアプリ (システムブラウザ) の流れで、Cookie の代わりに使い捨てチケットをアプリへ返す
        [HttpGet("login/{provider}")]
        public IActionResult ExternalLogin(string provider, string? returnUrl, bool mobile = false, bool persistent = false)
            => _externalLogins.Challenge(this, provider, returnUrl, mobile, persistent);

        //ネイティブアプリ: システムブラウザで受け取った使い捨てチケットを認証 Cookie に交換する
        [HttpPost("login_ticket")]
        public async Task<IActionResult> LoginTicket(LoginTicket? ticket)
        {
            var redeemed = await _externalLogins.RedeemMobileTicketAsync(ticket?.Ticket);
            if (redeemed == null) return Unauthorized();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                _externalLogins.CreatePrincipal(redeemed.Value.User, redeemed.Value.Provider),
                new AuthenticationProperties { IsPersistent = true });

            return Ok();
        }

        //外部 IdP (Entra ID 等) でサインインしたセッションは IdP 側のセッションも終わらせる必要があり、それはブラウザ遷移でしかできない
        //(fetch からは不可)。その場合は Cookie を残したまま ExternalLogout の URL を返し、クライアントがそこへ遷移する (二段構え)。
        //mobile=true (ネイティブアプリ) は Cookie を破棄するだけ
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(bool mobile = false)
        {
            if (!mobile)
            {
                var provider = await _externalLogins.GetSignOutProviderAsync(User);
                if (provider != null) return Ok(new LogoutResult { Redirect = $"/api/account/logout/{provider}" });
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new LogoutResult());
        }

        //Cookie を破棄し IdP のセッションも終わらせてログイン画面へ戻る (GET: ブラウザ遷移)
        [HttpGet("logout/{provider}")]
        public Task<IActionResult> ExternalLogout(string provider)
            => _externalLogins.SignOutAsync(this, provider, LoginPage);

        //ログイン中の自分の認証アプリ (TOTP) の登録状態と解除 (Extras の MyTotpResetButtonField が使う)。
        //対象は常に自分。列は書き込み専用なのでここが唯一の入口
        [Authorize]
        [HttpGet("totp/status")]
        public async Task<IActionResult> GetTotpStatus()
        {
            var totp = TotpLogin.Create(DesignerService.GetDesignData(), SystemConfig.Instance.TotpLogin, _dataService.DbAccess);
            if (totp == null) return Ok(new TotpStatus());
            var current = await totp.FindAsync(DataService.GetCurrentUserId(HttpContext));
            return Ok(new TotpStatus { Enabled = true, Registered = current?.IsConfirmed == true });
        }

        [Authorize]
        [HttpPost("totp/reset")]
        public async Task<IActionResult> TotpReset()
        {
            var totp = TotpLogin.Create(DesignerService.GetDesignData(), SystemConfig.Instance.TotpLogin, _dataService.DbAccess);
            if (totp == null) return NotFound();
            await totp.ResetAsync(DataService.GetCurrentUserId(HttpContext));
            return Ok(new TotpStatus { Enabled = true, Registered = false });
        }

        public class TotpStatus
        {
            public bool Enabled { get; set; }
            public bool Registered { get; set; }
        }
    }
}
