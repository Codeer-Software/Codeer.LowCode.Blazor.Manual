using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using LowCodeNativeSamples.Client.Shared.Services;

namespace LowCodeNativeSamples.Maui.Services
{
    public class NavigationService : NavigationServiceBase
    {
        readonly IHttpService _http;
        readonly NavigationManager _nav;
        readonly ServerConnection _server;

        public NavigationService(NavigationManager nav, IHttpService http, ServerConnection server) : base(nav)
        {
            _http = http;
            _nav = nav;
            _server = server;
        }

        public override bool CanLogout => true;

        public override async Task Logout()
        {
            //mobile=true: Cookie の破棄のみ。IdP 側セッションの終了はブラウザ遷移が要るのでアプリでは行わない
            await _http.PostAsJsonAsync("api/account/logout?mobile=true", "");
            _server.ResetAntiforgeryToken();
            _nav.NavigateTo("/login", true);
        }
    }
}
