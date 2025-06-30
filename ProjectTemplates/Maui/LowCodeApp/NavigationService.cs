using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using WebApp.Client.Shared.Services;

namespace LowCodeApp.Client
{
    public class NavigationService : NavigationServiceBase
    {
        public NavigationService(NavigationManager nav, IAppInfoService app) : base(nav, app) { }
        public override bool CanLogout => false;
        public override async Task Logout() => await Task.CompletedTask;
    }
}
