using System.Globalization;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeApp.WPF.Client.Shared.AITextAnalyzer;
using Microsoft.Extensions.DependencyInjection;
using Sotsera.Blazor.Toaster.Core.Models;

namespace LowCodeApp.WPF.LowCodeApp.Services
{
    public static class ServiceInitializer
    {
        public static void AddSharedServices(this IServiceCollection services)
        {
            services.AddScoped<IAppInfoService, AppInfoService>();
            services.AddScoped<IModuleDataService, ModuleDataService>();
            services.AddScoped<IUIService, LowCodeApp.WPF.Client.Shared.Services.UIService>();
            services.AddScoped<Codeer.LowCode.Blazor.RequestInterfaces.Services>();
            services.AddScoped<ILogger, LowCodeApp.WPF.Client.Shared.Services.Logger>();
            services.AddSingleton<LoadingService>();
            services.AddToaster(config =>
            {
                config.PositionClass = Defaults.Classes.Position.BottomRight;
                config.MaximumOpacity = 100;
                config.VisibleStateDuration = 1000 * 5;
                config.ShowTransitionDuration = 10;
                config.HideTransitionDuration = 500;
            });
            services.AddScoped<LowCodeApp.WPF.Client.Shared.Services.ToasterEx>();
            services.AddScoped<LowCodeApp.WPF.Client.Shared.Services.HttpService>();
            services.AddScoped<IAITextAnalyzerCore, AITextAnalyze>();

            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped(sp => new HttpClient());

            var cultureName = CultureInfo.CurrentCulture.Name;
            if (cultureName == "ja") cultureName = "ja-JP";
            var cultureInfo = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
