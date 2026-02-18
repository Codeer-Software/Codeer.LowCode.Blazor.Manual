using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Utils;
using CodeerLowCodeBlazorTemplate.Client.Shared.Services;
using CodeerLowCodeBlazorTemplate.Server.Services;
using CodeerLowCodeBlazorTemplate.Server.Services.FileManagement;
using Excel.Report.PDF;
using MessagePack;
using Microsoft.AspNetCore.Mvc;

namespace CodeerLowCodeBlazorTemplate.Server.Controllers
{
    [ApiController]
    [Route("api/module_data")]
    public class ModuleDataController : ControllerBase, IAsyncDisposable
    {
        readonly DataService _dataService;

        public ModuleDataController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        [HttpGet("config")]
        public SystemConfigForFront GetSystemConfig()
            => SystemConfig.Instance.ForFront();

        [HttpGet("design")]
        public async Task<IActionResult> GetDesignData()
        {
            await _dataService.ModuleDataIO.CheckAppAuthorization();
            return File(DesignerService.GetDesignDataForFront(await _dataService.ModuleDataIO.GetCurrentUser()), "application/octet-stream");
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListAsync(List<GetListRequest> request)
        {
            var ret = new List<Paging<ModuleData>>();
            foreach (var e in request)
            {
                ret.Add(await _dataService.ModuleDataIO.GetListAsync(e.Condition, e.PageIndex));
            }
            return Ok(new MemoryStream(MessagePackSerializer.Typeless.Serialize(ret)));
        }

        [HttpPost]
        public async Task<List<ModuleSubmitResult>> SubmitAsync(List<ModuleSubmitData>? data)
            => await _dataService.ModuleDataIO.SubmitWithTransactionAsync(data!);

        [HttpPost("excel_download")]
        public async Task<IActionResult> ExcelDownloadFileAsync(SearchCondition? condition)
            => Ok(ExcelUtils.CreateExcelBinary(await _dataService.ModuleDataIO.GetTableTextsAsync(condition!), "data"));

        [HttpPost("excel_upload")]
        public async Task<List<ModuleSubmitResult>> ExcelUploadFileAsync(string? moduleName)
        {
            var texts = await ExcelUtils.ReadAllTextsFromExcelBinary(Request.Body);
            if (500 < texts.Count) throw LowCodeException.Create("Excel has a maximum of 500 rows");
            return await _dataService.ModuleDataIO.SubmitWithTransactionByTableTextsAsync(moduleName, texts);
        }

        [HttpGet("resource")]
        public IActionResult GetResourceAsync(string? resource)
            => Ok(DesignerService.GetResource(resource ?? string.Empty));

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAsync(string? moduleName, string? id, string? fieldName)
        {
            var location = await _dataService.ModuleDataIO.FileFieldDataIO.GetFileLocation(moduleName!, id!, fieldName!);
            await _dataService.DbAccess.ClearAsync();
            return Ok(await StorageAccess.ReadFileAsync(location));
        }

        [HttpPost("upload")]
        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo> UploadFileAsync(string? moduleName, string? fieldName, string? fileName)
        {
            var info = _dataService.ModuleDataIO.FileFieldDataIO.GetFileSaveInfo(moduleName ?? string.Empty, fieldName ?? string.Empty);
            return await _dataService.TemporaryFileManager.AddFileAsync(info, fileName, Request.Body);
        }
    }
}
