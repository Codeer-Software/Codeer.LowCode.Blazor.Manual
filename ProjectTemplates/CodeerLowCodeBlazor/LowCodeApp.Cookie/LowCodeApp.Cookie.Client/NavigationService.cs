using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeApp.Cookie.Client.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace LowCodeApp.Cookie.Client
{
    public class NavigationService : NavigationServiceBase
    {
        readonly HttpService _http;
        readonly NavigationManager _nav;
        readonly IAppInfoService _appInfo;

        public NavigationService(NavigationManager nav, HttpService http, IAppInfoService appInfo) : base(nav)
        {
            _http = http;
            _nav = nav;
            _appInfo = appInfo;
        }

        public override bool CanLogout => true;

        public override async Task Logout()
        {
            await _http.PostAsJsonAsync("api/account/logout", "");
            _nav.NavigateTo("/", true);
        }
    }
}
