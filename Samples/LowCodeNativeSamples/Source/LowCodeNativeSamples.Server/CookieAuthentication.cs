using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using LowCodeNativeSamples.Server.Services;
using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.Extras.Server.Auth;

namespace LowCodeNativeSamples.Server
{
    public static class CookieAuthentication
    {
        public static void UseCookieAuthentication(this WebApplicationBuilder builder)
        {
            //ログイン途中の一時データ (外部 IdP の MAUI 向け使い捨てチケット、メール認証コード) の置き場。
            //これはプロセス内メモリなので単一インスタンス向け。App Service 等で複数インスタンスに広げるときは
            //AddStackExchangeRedisCache / AddDistributedSqlServerCache などの共有キャッシュに差し替える
            //(発行したインスタンスと検証するインスタンスが違うと失敗するため)
            builder.Services.AddDistributedMemoryCache();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                })
                //外部 IdP (Entra ID / Google / AWS Cognito / OIDC)。並べるプロバイダは Services/ExternalLoginTable が SystemConfig の設定から組み立てる。
                //IdP は本人確認をするだけで、セッションはこの Cookie のまま。確認できた本人をユーザー行に解決するのは ExternalLoginUserResolver
                .AddExternalLogins(ExternalLoginTable.Create(), o => o.MobileCallbackUrl = SystemConfig.Instance.MobileLoginCallbackUrl);
            builder.Services.AddScoped<IExternalLoginUserResolver, ExternalLoginUserResolver>();

            //CSRF
            builder.Services.AddAntiforgery(options => {
                options.HeaderName = "X-ANTIFORGERY-TOKEN";
            });
        }

        public static void UseCookieAuthentication(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            _ = CreateInitialUserAsync(app);
        }

        public static void AppendAntiforgeryTokenCookie(HttpContext ctx)
        {
            var anti = ctx.RequestServices.GetRequiredService<IAntiforgery>();
            var tokens = anti.GetAndStoreTokens(ctx);
            ctx.Response.Cookies.Append(
                "X-ANTIFORGERY-TOKEN",
                tokens.RequestToken ?? string.Empty,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
        }

        //ユーザーが 0 件なら admin / admin を作る (表・列はユーザーモジュールのデザインから。パスワードログインがある構成だけ)
        static async Task CreateInitialUserAsync(WebApplication app)
        {
            await using var dbAccessor = new DbAccessor(SystemConfig.Instance.DataSources);
            var accounts = LoginAccountStore.Create(DesignerService.GetDesignData(), dbAccessor);
            if (accounts == null || !accounts.HasPassword || await accounts.AnyAsync()) return;
            await accounts.AddAsync("admin", "admin");
        }
    }
}
