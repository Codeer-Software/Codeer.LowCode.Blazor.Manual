using System.Security.Claims;
using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.Utils;
using Dapper;
using LowCodeApp.Cookie.Client;
using LowCodeApp.Cookie.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeApp.Cookie.Server.Controllers
{
    [ApiController, AutoValidateAntiforgeryToken]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        readonly DataService _dataService;

        public AccountController(DataService dataService)
        {
            _dataService = dataService;
        }

        [Authorize]
        [HttpGet("current_user")]
        public StringWrapper GetCurrentUser()
            => new(DataService.GetCurrentUserId(HttpContext));

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginInfo? loginInfo)
        {
            if (loginInfo == null) throw new ArgumentException(nameof(loginInfo));

            var tableInfo = SystemConfig.Instance.PasswordCheckUserTableInfo;
            var designData = DesignerService.GetDesignData();

            var dataSourceName = designData.Modules.Find(designData.AppSettings.CurrentUserModuleDesignName)?.DataSourceName ?? string.Empty;

            var conn = _dataService.DbAccess.GetConnection(dataSourceName);

            var user = (await conn.QueryAsync<PasswordCheckUser>(
                $"SELECT {tableInfo.IdColumn} AS Id, {tableInfo.UserNameColumn} AS UserName, {tableInfo.HashColumn} AS Hash, {tableInfo.SaltColumn} AS Salt FROM {tableInfo.TableName} WHERE {tableInfo.UserNameColumn} = @UserName",
                new { UserName = loginInfo.Id })).FirstOrDefault();

            if (user == null) return Unauthorized();

            if (!PasswordHashHelper.VerifyHash(loginInfo.Password ?? string.Empty, user.Hash, user.Salt))
                return Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, loginInfo.Id ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = loginInfo.IsPersistent });

            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
