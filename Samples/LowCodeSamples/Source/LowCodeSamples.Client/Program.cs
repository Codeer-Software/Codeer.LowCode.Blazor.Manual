using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LowCodeSamples.Client;
using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeSamples.Client.Shared.Services;
using IgniteUI.Blazor.Controls;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

//デモサイトは認証を持たないため antiforgery トークンも無く、素の HttpClient でよい
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//サンプル固有: UI ライブラリ (Ignite UI) のサービス登録
builder.Services.AddIgniteUIBlazor();
builder.Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));

await builder.Build().RunAsync();
