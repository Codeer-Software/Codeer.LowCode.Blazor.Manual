using Codeer.LowCode.Blazor.Extras.Services;
using Dapper;
using LowCodeApp.Cookie.Server.Services;
using LowCodeApp.Cookie.Server.Shared;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LowCodeApp.Cookie.Server
{
    public static class CookieAuthentication
    {
        public static void UseCookieAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
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
                });

            //CSRF
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-ANTIFORGERY-TOKEN";
            });
        }

        public static void UseCookieAuthentication(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.Use(async (ctx, next) =>
            {
                if (HttpMethods.IsGet(ctx.Request.Method))
                {
                    if (ctx.Request.Headers.Accept.Any(a => a?.Contains("text/html") == true))
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
                }
                await next();
            });

            _ = CreateInitialUserAsync(app);
        }

        static string GetDataSourceName()
        {
            var designData = DesignerService.GetDesignData();
            return designData.Modules.Find(designData.AppSettings.CurrentUserModuleDesignName)?.DataSourceName ?? string.Empty;
        }

        static async Task CreateInitialUserAsync(WebApplication app)
        {
            var tableInfo = SystemConfig.Instance.PasswordCheckUserTableInfo;
            using var dbAccessor = new DbAccessor(SystemConfig.Instance.DataSources);
            var conn = dbAccessor.GetConnection(GetDataSourceName());

            var count = await conn.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM {tableInfo.TableName}");
            if (count > 0) return;

            var hashData = PasswordHashHelper.CreateHash("admin");

            await conn.ExecuteAsync(
                $"INSERT INTO {tableInfo.TableName} ({tableInfo.UserNameColumn}, {tableInfo.HashColumn}, {tableInfo.SaltColumn}) VALUES (@UserName, @Hash, @Salt)",
                new { UserName = "admin", Hash = hashData.Hash ?? string.Empty, Salt = hashData.Salt ?? string.Empty });
        }
    }
}
