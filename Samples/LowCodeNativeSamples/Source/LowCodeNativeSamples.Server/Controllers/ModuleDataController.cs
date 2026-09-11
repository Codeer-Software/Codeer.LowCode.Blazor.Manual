using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Utils;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LowCodeNativeSamples.Client.Shared.Services;
using LowCodeNativeSamples.Server.Services;
using Codeer.LowCode.Blazor.Extras.Server.BulkFile;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Web;

namespace LowCodeNativeSamples.Server.Controllers
{
    [Authorize, AutoValidateAntiforgeryToken]
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
            //FileFieldのDB列格納モードでファイル実体(byte[])を運ぶため、listの応答と同様にMessagePackで受ける
            using var memory = new MemoryStream();
            await Request.Body.CopyToAsync(memory);
            memory.Position = 0;
            var data = MessagePackSerializer.Typeless.Deserialize(memory) as List<ModuleSubmitData>;
            return await _dataService.ModuleDataIO.SubmitWithTransactionAsync(data!);
        }

        [HttpPost("list_file")]
        public async Task<IActionResult> GetListFileAsync(SearchCondition? condition)
            => Ok(await BulkFileTransfer.GetListFileAsync(DesignerService.GetDesignData(), _dataService.ModuleDataIO, condition!));

        [HttpPost("submit_by_file")]
        public async Task<List<ModuleSubmitResult>> SubmitByFileAsync(string? moduleName)
            => await BulkFileTransfer.SubmitByFileAsync(DesignerService.GetDesignData(), _dataService.ModuleDataIO, moduleName, Request.Body);

        //スクリプトの一括ファイル出力 (BulkFileTransferService.Download(List<Module>)) 用。
        //クライアントで加工済みのモジュールデータ列をそのままファイル化する
        [HttpPost("list_file_by_data")]
        public async Task<IActionResult> GetListFileByDataAsync(string? moduleName)
            => Ok(await BulkFileTransfer.GetListFileByDataAsync(DesignerService.GetDesignData(), _dataService.ModuleDataIO, moduleName, Request.Body));

        //スクリプトの一括保存 (BulkFileTransferService.Submit(List<Module>)) 用。
        //クライアントで加工済みのモジュールデータ列を一括保存する (ファイル取込と同じ追加/更新判定の経路)
        [HttpPost("bulk_submit")]
        public async Task<IActionResult> BulkSubmitAsync(string? moduleName)
            => Content(await BulkFileTransfer.BulkSubmitAsync(_dataService.ModuleDataIO, moduleName, Request.Body), "application/json");

        //スクリプトの一括ファイル取込 (BulkFileReader) 用。ファイルを解析してモジュールデータ列を返す (DB には書き込まない)。
        //ModuleData はポリモーフィックなので JsonConverterEx で直列化して返す
        [HttpPost("parse_file")]
        public async Task<IActionResult> ParseFileAsync(string? moduleName)
            => Content(Codeer.LowCode.Blazor.Json.JsonConverterEx.SerializeObject(
                await BulkFileTransfer.ParseFileAsync(DesignerService.GetDesignData(), _dataService.ModuleDataIO, moduleName, Request.Body)),
                "application/json");

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
            return this.FileWithETag((await StorageAccess.ReadFileAsync(FileStorageTable.Storages, location)).ToArray(), "application/octet-stream");
        }

        [HttpPost("upload")]
        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo> UploadFileAsync(string? moduleName, string? fieldName, string? fileName)
        {
            var info = _dataService.ModuleDataIO.FileFieldDataIO.GetFileSaveInfo(moduleName ?? string.Empty, fieldName ?? string.Empty);
            return await _dataService.TemporaryFileManager.AddFileAsync(info, fileName, Request.Body);
        }
    }
}
