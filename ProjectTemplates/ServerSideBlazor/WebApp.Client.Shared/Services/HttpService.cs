using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.Json;
using System.Text.Json.Serialization;

namespace WebApp.Client.Shared.Services
{
    public class HttpService
    {
        readonly HttpClient _http;
        readonly Codeer.LowCode.Blazor.RequestInterfaces.ILogger _logger;
        readonly LoadingService _loadingService;
        readonly List<ErrorCheckerScope> _checkers = new List<ErrorCheckerScope>();

        public HttpService(HttpClient http, LoadingService loadingService, Codeer.LowCode.Blazor.RequestInterfaces.ILogger logger)
        {
            _http = http;
            _logger = logger;
            _loadingService = loadingService;
        }

        public async Task<TValue?> GetFromJsonAsync<TValue>(string url, bool loading = true) where TValue : class
            => await ExecuteReturnJson<TValue>(async () => await _http.GetAsync(url), loading);

        public async Task<Stream?> GetFromStreamAsync(string url, bool loading = true)
            => await ExecuteReturnStream(async () => await _http.GetAsync(url), loading);

        public async Task<HttpResponseMessage?> GetAsync(string? requestUri, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.GetAsync(requestUri), loading);

        public async Task<HttpResponseMessage?> PostAsync(string url, HttpContent data, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.PostAsync(url, data), loading);

        public async Task<bool> PostAsJsonAsync<TValue>(string url, TValue data, bool loading = true)
             => await ExecuteReturnBool(async () => await _http.PostAsJsonAsync(url, data, CreateJsonOption()), loading);

        public async Task<TResult?> PostAsJsonAsync<TValue, TResult>(string url, TValue data, bool loading = true) where TResult : class
             => await ExecuteReturnJson<TResult>(async () => await _http.PostAsJsonAsync(url, data, CreateJsonOption()), loading);

        public async Task<TResult?> PostContentAsJsonAsync<TResult>(string requestUri, HttpContent? content, bool loading = true) where TResult : class
             => await ExecuteReturnJson<TResult>(async () => await _http.PostAsync(requestUri, content), loading);

        public async Task<HttpResponseMessage?> PostContent(string requestUri, HttpContent? content, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.PostAsync(requestUri, content), loading);

        public async Task<HttpResponseMessage?> PostAsJsonReturnHttpResponseAsync<TValue>(string url, TValue data, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.PostAsJsonAsync(url, data, CreateJsonOption()), loading);

        public async Task<HttpResponseMessage?> PutAsync(string url, HttpContent data, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.PutAsync(url, data), loading);

        public async Task<HttpResponseMessage?> DeleteAsync(string url, bool loading = true)
             => await ExecuteReturnHttpResponseMessage(async () => await _http.DeleteAsync(url), loading);

        async Task<bool> ExecuteReturnBool(Func<Task<HttpResponseMessage>> a, bool loading)
            => await ExecuteReturnHttpResponseMessage(a, loading) != null;

        async Task<T?> ExecuteReturnJson<T>(Func<Task<HttpResponseMessage>> a, bool loading) where T : class
        {
            using var scope = loading ? _loadingService.StartLoading() : null;
            var response = await ExecuteReturnHttpResponseMessage(a, loading);
            try
            {
                return response == null ? null : await response.Content.ReadFromJsonAsync<T>(CreateJsonOption());
            }
            catch (Exception e)
            {
                await Error(HttpStatusCode.InternalServerError, e.Message);
                return null;
            }
        }

        async Task<Stream?> ExecuteReturnStream(Func<Task<HttpResponseMessage>> a, bool loading)
        {
            using var scope = loading ? _loadingService.StartLoading() : null;
            var response = await ExecuteReturnHttpResponseMessage(a, loading);
            try
            {
                return response == null ? null : await response.Content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                await Error(HttpStatusCode.InternalServerError, e.Message);
                return null;
            }
        }

        async Task<HttpResponseMessage?> ExecuteReturnHttpResponseMessage(Func<Task<HttpResponseMessage>> a, bool loading)
        {
            using var scope = loading ? _loadingService.StartLoading() : null;
            try
            {
                var response = await a();
                if (!await CheckResponse(response)) return null;
                return response;
            }
            catch (Exception e)
            {
                await Error(HttpStatusCode.InternalServerError, e.Message);
                return null;
            }
        }

        async Task<bool> CheckResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return true;

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (response.Content.Headers?.ContentType?.MediaType == "text/plain")
                {
                    var error = await response.Content.ReadAsStringAsync();
                    if (error == null) error = "Invalid Communication";
                    await Error(HttpStatusCode.InternalServerError, error);
                    return false;
                }
            }

            await Error(response.StatusCode, $"Invalid Error Code{(int)response.StatusCode}");
            return false;
        }

        static JsonSerializerOptions CreateJsonOption()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.AddJsonConverters();
            return options;
        }

        async Task Error(HttpStatusCode code, string message)
        {
            if (_checkers.Any()) _checkers.ForEach(e => e.Error(code, message));
            else await _logger.Error(message);
        }

        public IDisposable AddChecker(Action<HttpStatusCode, string> check)
            => new ErrorCheckerScope(check, _checkers);

        class ErrorCheckerScope : IDisposable
        {
            List<ErrorCheckerScope> _checkers;
            Action<HttpStatusCode, string> _check;

            public ErrorCheckerScope(Action<HttpStatusCode, string> check, List<ErrorCheckerScope> checkers)
            {
                _check = check;
                _checkers = checkers;
                _checkers.Add(this);
            }

            public void Dispose() => _checkers.Remove(this);

            public void Error(HttpStatusCode code, string message)
                => _check(code, message);
        }
    }
}
