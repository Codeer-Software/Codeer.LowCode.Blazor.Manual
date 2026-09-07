namespace LowCodeSamples.Maui.Services
{
    /// <summary>
    /// Connection to the LowCodeSamples server. The demo server has no authentication, so unlike the Cookie
    /// authentication template there is no cookie jar and no antiforgery token to carry: this only creates the
    /// HttpClient for the current server URL (see ServerSettings).
    /// </summary>
    public class ServerConnection
    {
        /// <summary>Current server URL (see ServerSettings). Read when an HttpClient is created.</summary>
        public Uri BaseAddress => new(ServerSettings.BaseUrl);

        public HttpClient CreateHttpClient()
        {
            //A new client is created per BlazorWebView, so a changed server URL is picked up after the page restarts.
            var handler = new HttpClientHandler();
#if DEBUG
            //The ASP.NET Core development certificate is issued for "localhost" and is not trusted by the device,
            //while the emulator reaches the PC as 10.0.2.2. Accept any certificate in Debug builds only.
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            //The default 100 second timeout makes a wrong BaseUrl look like a hang.
            return new HttpClient(handler) { BaseAddress = BaseAddress, Timeout = TimeSpan.FromSeconds(30) };
        }
    }
}
