using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using LowCodeSamples.Client.Shared.Services;

namespace LowCodeSamples.Maui.Services
{
    public class NavigationService : NavigationServiceBase
    {
        public NavigationService(NavigationManager nav) : base(nav) { }

        //デモサイトは認証を持たないためログアウトは無い
        public override bool CanLogout => false;

        public override Task Logout() => Task.CompletedTask;
    }
}
