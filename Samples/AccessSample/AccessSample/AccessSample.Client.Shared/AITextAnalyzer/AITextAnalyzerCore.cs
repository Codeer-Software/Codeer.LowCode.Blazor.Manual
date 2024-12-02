using AccessSample.Client.Shared.Services;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Data;

namespace AccessSample.Client.Shared.AITextAnalyzer
{
    public class AITextAnalyzerCore : IAITextAnalyzerCore
    {
        HttpService _httpService;

        public AITextAnalyzerCore(HttpService httpService)
            => _httpService = httpService;

        public async Task<ModuleData?> FileToModuleDataAsync(string moduleName, string fileName, StreamContent content)
            => await _httpService.PostContentAsJsonAsync<ModuleData>(
                $"/api/ai_text_analyze/file?moduleName={moduleName}&fileName={fileName}", content);

        public async Task<ModuleData?> TextToModuleDataAsync(string moduleName, string text)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "text", text } });
            var ret = await _httpService.PostAsync($"/api/ai_text_analyze/text?moduleName={moduleName}", content);
            if (ret?.IsSuccessStatusCode != true) return null;
            return JsonConverterEx.DeserializeObject<ModuleData>(await ret.Content.ReadAsStringAsync());
        }
    }
}
