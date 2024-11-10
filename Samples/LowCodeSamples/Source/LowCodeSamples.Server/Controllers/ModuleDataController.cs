using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.Utils;
using Excel.Report.PDF;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.FileManagement;
using LowCodeSamples.Server.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
{
  [ApiController]
  [Route("api/module_data")]
  public class ModuleDataController : ControllerBase, IAuthenticationContext, IAsyncDisposable
  {
    readonly DbAccessor _dbAccess;
    readonly TemporaryFileManager _temporaryFileManager;
    readonly CustomizedModuleDataIO _moduleDataIO;

    public ModuleDataController()
    {
      _dbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
      _temporaryFileManager = new TemporaryFileManager(_dbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
      _moduleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), this, _dbAccess, _temporaryFileManager);
    }

    public async ValueTask DisposeAsync()
        => await _dbAccess.DisposeAsync();

    [HttpGet("use_hot_reload")]
    public ValueWrapper<bool> IsUseHotReload()
        => new ValueWrapper<bool>(SystemConfig.Instance.UseHotReload);

    [HttpGet("design")]
    public async Task<IActionResult> GetDesignData()
    {
      await _moduleDataIO.CheckAppAuthorization();
      return File(DesignerService.GetDesignDataForFront(await _moduleDataIO.GetCurrentUser()), "application/octet-stream");
    }

    [HttpPost("list")]
    public async Task<Paging<ModuleData>> GetListAsync(int? page, SearchCondition? condition)
        => await _moduleDataIO.GetListAsync(condition!, page ?? 0);

    [HttpPost]
    public async Task<List<ModuleSubmitResult>> SubmitAsync(List<ModuleSubmitData>? data)
    {
      if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
      return await _moduleDataIO.SubmitWithTransactionAsync(data!);
    }

    [HttpPost("excel_download")]
    public async Task<IActionResult> ExcelDownloadFileAsync(SearchCondition? condition)
        => Ok(ExcelUtils.CreateExcelBinary(await _moduleDataIO.GetTableTextsAsync(condition!), "data"));

    [HttpPost("excel_upload")]
    public async Task<List<ModuleSubmitResult>> ExcelUploadFileAsync(string? moduleName)
    {
      if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
      var texts = await ExcelUtils.ReadAllTextsFromExcelBinary(Request.Body);
      if (500 < texts.Count) throw LowCodeException.Create("Excel has a maximum of 500 rows");
      return await _moduleDataIO.SubmitWithTransactionByTableTextsAsync(moduleName, texts);
    }

    [HttpGet("resource")]
    public IActionResult GetResourceAsync(string? resource)
        => Ok(DesignerService.GetResource(resource ?? string.Empty));

    [HttpGet("download")]
    public async Task<IActionResult> DownloadFileAsync(string? moduleName, string? id, string? fieldName)
    {
      var location = await _moduleDataIO.FileFieldDataIO.GetFileLocation(moduleName!, id!, fieldName!);
      await _dbAccess.ClearAsync();
      return Ok(await StorageAccess.ReadFileAsync(location));
    }

    [HttpPost("upload")]
    public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo> UploadFileAsync(string? moduleName, string? fieldName, string? fileName)
    {
      if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
      var info = _moduleDataIO.FileFieldDataIO.GetFileSaveInfo(moduleName ?? string.Empty, fieldName ?? string.Empty);
      return await _temporaryFileManager.AddFileAsync(info, fileName, Request.Body);
    }

    public async Task<string> GetCurrentUserIdAsync()
    {
      await Task.CompletedTask;
      return string.Empty;
    }
  }
}
