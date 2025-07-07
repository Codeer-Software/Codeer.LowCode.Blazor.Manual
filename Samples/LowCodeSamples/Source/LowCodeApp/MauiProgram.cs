using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeApp.Client;
using LowCodeSamples.Client.Shared.Samples.MobileSensor;
using LowCodeSamples.Client.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace LowCodeApp
{
    public static class MauiProgram
    {
        private static string _baseUrl = string.Empty;
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSharedServices();
            builder.Services.AddScoped<INavigationService, NavigationService>();

            var cfgBuilder = new ConfigurationBuilder();
            var baseStream = FileSystem
                 .OpenAppPackageFileAsync("appsettings.json")
                 .Result;
            cfgBuilder.AddJsonStream(baseStream);

            Stream? devStream = null;
            try
            {
                devStream = FileSystem
                    .OpenAppPackageFileAsync("appsettings.Development.json")
                    .Result;
                cfgBuilder.AddJsonStream(devStream);
            }
            catch { }

            var config = cfgBuilder.Build();
            builder.Configuration.AddConfiguration(config);

            baseStream.Dispose();
            devStream?.Dispose();

            _baseUrl = builder.Configuration["Network:BaseUrl"] ?? string.Empty;

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(_baseUrl) });
            builder.Services.AddSingleton<IGeolocationService, MauiGeolocationService>();
            builder.Services.AddSingleton<IAccelerometerService, MauiAccelerometerService>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, bundle) =>
                {
                    _ = CheckLicenseAsync();
                }));
#endif
#if IOS
                events.AddiOS(ios => ios.FinishedLaunching((app, options) =>
                {
                    _ = CheckLicenseAsync();
                    return true;
                }));
#endif
#if WINDOWS
                events.AddWindows(windows => windows.OnLaunched((app, args) =>
                {
                    _ = CheckLicenseAsync();
                }));
#endif
            });

            return builder.Build();
        }

        static async Task CheckLicenseAsync()
        {
            using var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            await client.PostAsync("api/license/update_license", new StringContent(""));
        }
    }
}
