using AccessSample.Client.Shared.Services;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;

namespace AccessSample.Client
{
    public class NavigationService : NavigationServiceBase
    {
        public NavigationService(NavigationManager nav, IAppInfoService app) : base(nav, app) { }
        public override bool CanLogout => false;
        public override async Task Logout() => await Task.CompletedTask;
    }
}
