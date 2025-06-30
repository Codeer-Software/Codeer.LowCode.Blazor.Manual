using Codeer.LowCode.Blazor.RequestInterfaces;
using Sotsera.Blazor.Toaster.Core.Models;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using WebApp.Client.Shared.AITextAnalyzer;

namespace WebApp.Client.Shared.Services
{
    public static class ServiceInitializer
    {
        public static void AddSharedServices(this IServiceCollection services)
        {
            services.AddScoped<IAppInfoService, AppInfoService>();
            services.AddScoped<IModuleDataService, ModuleDataService>();
            services.AddScoped<IUIService, UIService>();
            services.AddScoped<Codeer.LowCode.Blazor.RequestInterfaces.Services>();
            services.AddScoped<ILogger, Logger>();
            services.AddSingleton<LoadingService>();
            services.AddToaster(config =>
            {
                config.PositionClass = Defaults.Classes.Position.BottomRight;
                config.MaximumOpacity = 100;
                config.VisibleStateDuration = 1000 * 5;
                config.ShowTransitionDuration = 10;
                config.HideTransitionDuration = 500;
            });
            services.AddScoped<ToasterEx>();
            services.AddScoped<HttpService>();
            services.AddScoped<IAITextAnalyzerCore, AITextAnalyzerCore>();

            var cultureName = CultureInfo.CurrentCulture.Name;
            if (cultureName == "ja") cultureName = "ja-JP";
            var cultureInfo = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
