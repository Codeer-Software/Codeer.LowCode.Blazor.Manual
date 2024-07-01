using CDataSample.Client.Shared.Services;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Script;
using System.Text;

namespace CDataSample.Client.Shared.ScriptObjects
{
    public class WebApiService
    {
        readonly HttpService _http;
        readonly ILogger _logger;

        public WebApiService(HttpService http, ILogger logger)
        {
            _http = http;
            _logger = logger;
        }

        [ScriptName("Get")]
        public async Task<WebApiResult> GetAsync(string url)
            => await ToWebResult(async () => await _http.GetAsync(url));

        [ScriptName("Post")]
        public async Task<WebApiResult> PostAsync(string url, JsonObject data)
            => await ToWebResult(async () => await _http.PostAsync(url, new StringContent(JsonConverterEx.SerializeObject(data), Encoding.UTF8, "application/json")));

        [ScriptName("Put")]
        public async Task<WebApiResult> PutAsync(string url, JsonObject data)
             => await ToWebResult(async () => await _http.PutAsync(url, new StringContent(JsonConverterEx.SerializeObject(data), Encoding.UTF8, "application/json")));

        [ScriptName("Delete")]
        public async Task<WebApiResult> DeleteAsync(string url)
             => await ToWebResult(async () => await _http.DeleteAsync(url));

        async Task<WebApiResult> ToWebResult(Func<Task<HttpResponseMessage?>> core)
        {
            int code = 200;
            var text = string.Empty;
            HttpResponseMessage? response = null;
            using (_http.AddChecker((code_, text_) => { code = (int)code_; text = text_; }))
            {
                response = await core();
            }

            var result = new WebApiResult { StatusCode = code };
            if (response != null)
            {
                result.JsonObject = await GetJsonObject(response.Content);
                result.StatusCode = (int)response.StatusCode;
            }
            else
            {
                await _logger.Error(text);
            }
            return result;
        }

        async Task<JsonObject> GetJsonObject(HttpContent content)
        {
            var resultText = await content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(resultText)) return new();
            else
            {
                try
                {
                    return JsonConverterEx.ToJsonObject(resultText);
                }
                catch (Exception e)
                {
                    await _logger.Error(e.Message);
                    return new();
                }
            }
        }
    }
}
