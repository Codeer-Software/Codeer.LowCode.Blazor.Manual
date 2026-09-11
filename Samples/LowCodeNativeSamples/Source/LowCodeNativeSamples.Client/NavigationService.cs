using Codeer.LowCode.Blazor.Extras.Services;
using Microsoft.AspNetCore.Components;
using LowCodeNativeSamples.Client.Shared.Services;
using Codeer.LowCode.Blazor.RequestInterfaces;

namespace LowCodeNativeSamples.Client
{
    public class NavigationService : NavigationServiceBase
    {
        readonly IHttpService _http;
        readonly NavigationManager _nav;
        readonly IAppInfoService _appInfo;

        public NavigationService(NavigationManager nav, IHttpService http, IAppInfoService appInfo) : base(nav)
        {
            _http = http;
            _nav = nav;
            _appInfo = appInfo;
        }

        public override bool CanLogout => true;

        public override async Task Logout()
        {
            //外部 IdP (Entra ID 等) でサインインしたセッションは IdP へのブラウザ遷移で終わらせる必要があり、
            //その場合サーバーは Cookie を消さずに遷移先 URL を返す (二段構え)
            var result = await _http.PostAsJsonAsync<string, LogoutResult>("api/account/logout", "");
            _nav.NavigateTo(string.IsNullOrEmpty(result?.Redirect) ? "/" : result.Redirect, true);
        }
    }
}
