using AccessSample.Client;
using AccessSample.Client.Shared;
using AccessSample.Client.Shared.Services;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<AfterBodyOutlet>("body::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

using (var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    await client.PostAsync("api/license/update_license", new StringContent(""));
}

await builder.Build().RunAsync();
