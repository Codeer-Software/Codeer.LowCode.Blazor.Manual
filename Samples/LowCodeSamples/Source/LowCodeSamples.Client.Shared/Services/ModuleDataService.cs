using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Utils;

namespace LowCodeSamples.Client.Shared.Services
{
    public class ModuleDataService : IModuleDataService
    {
        readonly HttpService _http;

        public ModuleDataService(HttpService http)
            => _http = http;

        public async Task<Paging<ModuleData>> GetListAsync(SearchCondition condition, int pageIndex)
            => await _http.PostAsJsonAsync<SearchCondition, Paging<ModuleData>>($"/api/module_data/list?page={pageIndex}", condition) ?? new();

        public async Task<List<ModuleSubmitResult>?> SubmitAsync(List<ModuleSubmitData> data)
            => await _http.PostAsJsonAsync<List<ModuleSubmitData>, List<ModuleSubmitResult>>($"/api/module_data", data);

        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo?> UploadFile(string moduleName, string fieldName, string fileName, StreamContent content)
            => await _http.PostContentAsJsonAsync<Codeer.LowCode.Blazor.DataIO.FileInfo>($"/api/module_data/upload?moduleName={moduleName}&fieldName={fieldName}&fileName={fileName}", content);

        public async Task<MemoryStream?> DownloadFile(string moduleName, string fieldName, string id)
        {
            var result = await _http.GetAsync($"/api/module_data/download?moduleName={moduleName}&fieldName={fieldName}&id={id}", false);
            if (result == null) return null;
            return (MemoryStream)await result.Content.ReadAsStreamAsync();
        }

        public async Task<MemoryStream?> GetListByExcelFileAsync(SearchCondition condition)
        {
            var result = await _http.PostAsJsonReturnHttpResponseAsync($"/api/module_data/excel_download", condition);
            if (result == null) return null;
            return (MemoryStream)await result.Content.ReadAsStreamAsync();
        }

        public async Task<List<ModuleSubmitResult>?> SubmitByExcelFileAsync(string moduleName, StreamContent content)
            => await _http.PostContentAsJsonAsync<List<ModuleSubmitResult>>($"/api/module_data/excel_upload?moduleName={moduleName}", content);
    }
}
