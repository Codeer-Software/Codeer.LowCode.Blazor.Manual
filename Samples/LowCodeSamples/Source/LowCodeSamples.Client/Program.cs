using Codeer.LowCode.Bindings.MudBlazor.Designs;
using Codeer.LowCode.Bindings.Radzen.Blazor.Designs;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Heron.MudCalendar;
using IgniteUI.Blazor.Controls;
using LowCodeSamples.Client;
using LowCodeSamples.Client.Shared;
using LowCodeSamples.Client.Shared.Samples.ColorPicker;
using LowCodeSamples.Client.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Radzen;

//load dll.
typeof(ColorPickerField).ToString();
typeof(MudBlazor.Variant).ToString();
typeof(MudTextFieldDesign).ToString();
typeof(MudCalendar).ToString();
typeof(Radzen.Variant).ToString();
typeof(RadzenTextFieldDesign).ToString();
new MudChart();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<AfterBodyOutlet>("body::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddRadzenComponents();

using (var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
  await client.PostAsync("api/license/update_license", new StringContent(""));
}

builder.Services.AddIgniteUIBlazor();
builder.Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));

await builder.Build().RunAsync();
