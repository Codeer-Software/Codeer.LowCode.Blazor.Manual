using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Utils;
using Excel.Report.PDF;
using LowCodeSamples.Client.Shared.Services;
using LowCodeSamples.Server.Services;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Web;
using MessagePack;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
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
            await LicenseService.UpdateAsync(Request);
            await _dataService.ModuleDataIO.CheckAppAuthorization();
            return this.FileWithETag(DesignerService.GetDesignDataForFront(await _dataService.ModuleDataIO.GetCurrentUser()), "application/octet-stream");
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListAsync(List<GetListRequest> request)
        {
            var ret = new List<Paging<ModuleData>>();
            foreach (var e in request)
            {
                ret.Add(await _dataService.ModuleDataIO.GetListAsync(e.Condition, e.PageIndex));
            }
            return File(new MemoryStream(MessagePackSerializer.Typeless.Serialize(ret)), "application/octet-stream");
        }

        [HttpPost]
        public async Task<List<ModuleSubmitResult>> SubmitAsync()
        {
            if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
            //FileFieldのDB列格納モードでファイル実体(byte[])を運ぶため、listの応答と同様にMessagePackで受ける
            using var memory = new MemoryStream();
            await Request.Body.CopyToAsync(memory);
            memory.Position = 0;
            var data = MessagePackSerializer.Typeless.Deserialize(memory) as List<ModuleSubmitData>;
            return await _dataService.ModuleDataIO.SubmitWithTransactionAsync(data!);
        }

        [HttpPost("excel_download")]
        public async Task<IActionResult> ExcelDownloadFileAsync(SearchCondition? condition)
            => Ok(ExcelUtils.CreateExcelBinary(await _dataService.ModuleDataIO.GetTableTextsAsync(condition!), "data"));

        [HttpPost("excel_upload")]
        public async Task<List<ModuleSubmitResult>> ExcelUploadFileAsync(string? moduleName)
        {
            if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
            var texts = await ExcelUtils.ReadAllTextsFromExcelBinary(Request.Body);
            if (500 < texts.Count) throw LowCodeException.Create("Excel has a maximum of 500 rows");
            return await _dataService.ModuleDataIO.SubmitWithTransactionByTableTextsAsync(moduleName, texts);
        }

        [HttpGet("resource")]
        public IActionResult GetResourceAsync(string? resource)
        {
            var mem = DesignerService.GetResource(resource ?? string.Empty);
            return mem == null ? Ok() : this.FileWithETag(mem.ToArray(), "application/octet-stream");
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileAsync(string? moduleName, string? id, string? fieldName)
        {
            var location = await _dataService.ModuleDataIO.FileFieldDataIO.GetFileLocation(moduleName!, id!, fieldName!);
            await _dataService.DbAccess.ClearAsync();
            return this.FileWithETag((await StorageAccess.ReadFileAsync(SystemConfig.Instance.FileStorages, location)).ToArray(), "application/octet-stream");
        }

        [HttpPost("upload")]
        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo> UploadFileAsync(string? moduleName, string? fieldName, string? fileName)
        {
            if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
            var info = _dataService.ModuleDataIO.FileFieldDataIO.GetFileSaveInfo(moduleName ?? string.Empty, fieldName ?? string.Empty);
            return await _dataService.TemporaryFileManager.AddFileAsync(info, fileName, Request.Body);
        }
    }
}
