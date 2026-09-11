using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeNativeSamples.Maui.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LowCodeNativeSamples.Client.Shared.Services;

namespace LowCodeNativeSamples.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Configuration.AddConfiguration(LoadAppSettings());

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSharedServices();
            builder.Services.AddScoped<INavigationService, NavigationService>();

            //All HTTP traffic goes to the server URL from ServerSettings (appsettings.json default, overridable
            //from the native SettingsPage). ServerConnection keeps the authentication cookie and the antiforgery token.
            var baseUrl = builder.Configuration["Server:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("Server:BaseUrl is not set in appsettings.json.");
            ServerSettings.DefaultBaseUrl = baseUrl;
            ServerSettings.LoginCallbackUrl = builder.Configuration["Server:LoginCallbackUrl"] ?? ServerSettings.LoginCallbackUrl;
            builder.Services.AddSingleton<ServerConnection>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<ServerConnection>().CreateHttpClient());

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        //appsettings.json is bundled as a MauiAsset. appsettings.Development.json (optional) overrides it.
        static IConfiguration LoadAppSettings()
        {
            var config = new ConfigurationBuilder();
            AddJsonAsset(config, "appsettings.json", optional: false);
            AddJsonAsset(config, "appsettings.Development.json", optional: true);
            return config.Build();
        }

        static void AddJsonAsset(IConfigurationBuilder config, string fileName, bool optional)
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
                var memory = new MemoryStream();
                stream.CopyTo(memory);
                memory.Position = 0;
                config.AddJsonStream(memory);
            }
            catch (FileNotFoundException) when (optional) { }
        }
    }
}
