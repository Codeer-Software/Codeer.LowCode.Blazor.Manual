using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LowCodeNativeSamples.Client;
using Microsoft.JSInterop;
using Codeer.LowCode.Blazor.RequestInterfaces;
using LowCodeNativeSamples.Client.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

builder.Services.AddScoped(sp => {
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

// The antiforgery token cookie is issued by GET /api/account/antiforgery.
// Fetch it before the app starts so the HttpClient factory above can read it from the cookie.
using (var boot = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    try { await boot.GetAsync("api/account/antiforgery"); } catch { }
}

await builder.Build().RunAsync();
