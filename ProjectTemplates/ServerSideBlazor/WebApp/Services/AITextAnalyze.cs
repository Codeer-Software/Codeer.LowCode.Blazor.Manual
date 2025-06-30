using Codeer.LowCode.Blazor.Repository.Data;
using WebApp.Client.Shared.AITextAnalyzer;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using WebApp.Services.AI;
using WebApp.Server.Shared;
using WebApp.Services.FileManagement;

namespace WebApp.Services
{
    public class AITextAnalyze : IAITextAnalyzerCore
    {
        LoadingService _loadingService;
        Codeer.LowCode.Blazor.RequestInterfaces.ILogger _logger;

        public AITextAnalyze(LoadingService loadingService, Codeer.LowCode.Blazor.RequestInterfaces.ILogger logger)
        {
            _loadingService = loadingService;
            _logger = logger;
        }

        public async Task<ModuleData?> FileToModuleDataAsync(string moduleName, string fileName, StreamContent content)
              => await CheckoutException(async dataIO =>
              {
                  var memoryStream = new MemoryStream();
                  await content.CopyToAsync(memoryStream);
                  memoryStream.Position = 0;
                  return await AITextAnalyzeService.FileToDataAsync(dataIO, moduleName, fileName, memoryStream);
              }, null);

        public async Task<ModuleData?> TextToModuleDataAsync(string moduleName, string text)
            => await CheckoutException(async dataIO => await AITextAnalyzeService.TextToDataAsync(dataIO, moduleName, text ?? string.Empty), null);

        async Task<T> CheckoutException<T>(Func<CustomizedModuleDataIO, Task<T>> f, T errResult)
        {
            using var scope = _loadingService.StartLoading();
            await using var dbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            var temporaryFileManager = new TemporaryFileManager(dbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
            var dataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), new AuthenticationContext(), dbAccess, temporaryFileManager);
            try
            {
                return await f(dataIO);
            }
            catch
            {
                await _logger.Error("AI analysis failed. Retrying may succeed.");
                return errResult;
            }
        }
    }
}
