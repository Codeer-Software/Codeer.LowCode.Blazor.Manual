using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeApp.Cookie.Client;
using LowCodeApp.Cookie.Client.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    var jsRuntime = (IJSInProcessRuntime)sp.GetService<IJSRuntime>()!;
    var cookie = jsRuntime.Invoke<string>("window.jsFunctions.getCookie");
    var token = cookie.Split(";").Select(e => e.Trim().Split("=")).Where(e => e.Length == 2 && e[0] == "X-ANTIFORGERY-TOKEN").Select(e => e[1]).FirstOrDefault();
    httpClient.DefaultRequestHeaders.Add("X-ANTIFORGERY-TOKEN", token);

    return httpClient;
});

using (var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    await client.PostAsync("api/license/update_license", new StringContent(""));
}

await builder.Build().RunAsync();
