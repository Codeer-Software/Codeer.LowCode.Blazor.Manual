using LowCodeApp.WinForms.Client.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace LowCodeApp.WinForms.LowCodeApp.Services
{
    public class NavigationService : NavigationServiceBase
    {
        public NavigationService(NavigationManager nav) : base(nav) { }
        public override bool CanLogout => false;
        public override async Task Logout() => await Task.CompletedTask;
    }
}
