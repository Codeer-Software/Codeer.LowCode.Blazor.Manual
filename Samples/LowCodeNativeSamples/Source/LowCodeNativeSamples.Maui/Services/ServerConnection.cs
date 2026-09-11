using System.Net;

namespace LowCodeNativeSamples.Maui.Services
{
    /// <summary>
    /// Connection to the Codeer.LowCode.Blazor server (the Cookie authentication variant).
    /// Holds the cookie jar (authentication cookie + antiforgery cookie) and the antiforgery
    /// request token that the server expects in the X-ANTIFORGERY-TOKEN header.
    /// In the browser these are handled by the browser itself; in a native app we do it here.
    /// </summary>
    public class ServerConnection
    {
        const string AntiforgeryTokenName = "X-ANTIFORGERY-TOKEN";

        readonly CookieContainer _cookies = new();
        string? _antiforgeryToken;

        /// <summary>Current server URL (see ServerSettings). Read when an HttpClient is created.</summary>
        public Uri BaseAddress => new(ServerSettings.BaseUrl);

        public HttpClient CreateHttpClient()
        {
            //A new client is created per BlazorWebView, so a changed server URL is picked up after the page restarts.
            _antiforgeryToken = null;
            var inner = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookies
            };
#if DEBUG
            //The ASP.NET Core development certificate is issued for "localhost" and is not trusted by the device,
            //while the emulator reaches the PC as 10.0.2.2. Accept any certificate in Debug builds only.
            inner.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            var handler = new AntiforgeryHandler(this) { InnerHandler = inner };
            //The default 100 second timeout makes a wrong BaseUrl look like a hang.
            return new HttpClient(handler) { BaseAddress = BaseAddress, Timeout = TimeSpan.FromSeconds(30) };
        }

        /// <summary>
        /// The antiforgery token is bound to the signed-in user. Call this after login/logout so the next
        /// request fetches a token for the new identity.
        /// </summary>
        public void ResetAntiforgeryToken() => _antiforgeryToken = null;

        //The server issues the token as a readable cookie from GET api/account/antiforgery (and marks it Secure,
        //so the cookie container would not send it back over http). We read it from the Set-Cookie header instead.
        void CaptureAntiforgeryToken(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookies)) return;
            foreach (var setCookie in setCookies)
            {
                var first = setCookie.Split(';')[0].Trim();
                var eq = first.IndexOf('=');
                if (eq <= 0) continue;
                if (first.Substring(0, eq) != AntiforgeryTokenName) continue;
                _antiforgeryToken = Uri.UnescapeDataString(first.Substring(eq + 1));
            }
        }

        class AntiforgeryHandler : DelegatingHandler
        {
            readonly ServerConnection _connection;

            public AntiforgeryHandler(ServerConnection connection)
            {
                _connection = connection;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                //AutoValidateAntiforgeryToken only validates unsafe methods.
                if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head && request.Method != HttpMethod.Options)
                {
                    var token = await GetAntiforgeryTokenAsync(cancellationToken);
                    if (token != null)
                    {
                        request.Headers.Remove(AntiforgeryTokenName);
                        request.Headers.Add(AntiforgeryTokenName, token);
                    }
                }

                var response = await base.SendAsync(request, cancellationToken);
                _connection.CaptureAntiforgeryToken(response);
                return response;
            }

            async Task<string?> GetAntiforgeryTokenAsync(CancellationToken cancellationToken)
            {
                if (_connection._antiforgeryToken != null) return _connection._antiforgeryToken;
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_connection.BaseAddress, "api/account/antiforgery"));
                using var response = await base.SendAsync(request, cancellationToken);
                _connection.CaptureAntiforgeryToken(response);
                return _connection._antiforgeryToken;
            }
        }
    }
}
