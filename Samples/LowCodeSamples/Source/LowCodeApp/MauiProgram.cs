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
                events.AddAndroid(android => android
                    // 起動時
                    .OnCreate((activity, bundle) =>
                    {
                        _ = CheckLicenseAsync();
                    })
                    // Activity がバックグラウンド→フォアグラウンドに来る前
                    .OnStart(activity =>
                    {
                    })
                    // Activity が完全にフォアグラウンドになった直後
                    .OnResume(activity =>
                    {
                    })
                    // 電源オフや他アプリ遷移で非アクティブになるとき
                    .OnPause(activity =>
                    {
                    })
                    // バックグラウンドに入ったとき
                    .OnStop(activity =>
                    {
                    })
                );
#endif
#if IOS
                events.AddiOS(ios => ios
                    // 起動完了後
                    .FinishedLaunching((app, options) =>
                    {
                        _ = CheckLicenseAsync();
                        return true;
                    })
                    // バックグラウンドやロック状態から復帰し、アクティブ状態になった直後
                    .OnActivated(app =>
                    {
                    })
                    // 電源オフ／着信などで非アクティブになる直前
                    .OnResignActivation(app =>
                    {
                    })
                    // バックグラウンド状態に完全に移行したとき
                    .DidEnterBackground(app =>
                    {
                    })
                    // バックグラウンドからフォアグラウンドに戻る直前
                    .WillEnterForeground(app =>
                    {
                    })
                );
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
