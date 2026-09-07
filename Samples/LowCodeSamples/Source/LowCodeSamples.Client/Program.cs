using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LowCodeSamples.Client;
using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeSamples.Client.Shared.Services;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using IgniteUI.Blazor.Controls;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Radzen;

//load dll. (サンプル固有: UI ライブラリのバインディング)
typeof(FluentTextFieldDesign).ToString();
typeof(Appearance).ToString();
MudBlazorLoader.LoadAssemblies();
RadzenLoader.LoadAssemblies();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

//デモサイトは認証を持たないため antiforgery トークンも無く、素の HttpClient でよい
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//サンプル固有: UI ライブラリのサービス登録
builder.Services.AddFluentUIComponents();
builder.Services.AddMudServices();
builder.Services.AddRadzenComponents();
builder.Services.AddIgniteUIBlazor();
builder.Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));

await builder.Build().RunAsync();
