using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Utils;
using Excel.Report.PDF;
using WebApp.Server.Shared;
using WebApp.Services.FileManagement;

namespace WebApp.Services
{
    public class ModuleDataService : IModuleDataService
    {
        Codeer.LowCode.Blazor.RequestInterfaces.ILogger _logger;

        public ModuleDataService(Codeer.LowCode.Blazor.RequestInterfaces.ILogger logger)
            => _logger = logger;

        public async Task<Paging<ModuleData>> GetListAsync(SearchCondition condition, int pageIndex, bool withLock)
            => await CheckoutException(async dataIO => await dataIO.GetListAsync(condition!, pageIndex), new());

        public async Task<List<ModuleSubmitResult>?> SubmitAsync(List<ModuleSubmitData> data)
            => await CheckoutException(async dataIO => await dataIO.SubmitWithTransactionAsync(data), null);

        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo?> UploadFile(string moduleName, string fieldName, string fileName, StreamContent content)
            => await CheckoutException(async dataIO => {

                var moduleDataIO = dataIO;
                var info = moduleDataIO.FileFieldDataIO.GetFileSaveInfo(moduleName ?? string.Empty, fieldName ?? string.Empty);
                using (var stream = content.ReadAsStream())
                {
                    return await moduleDataIO.TemporaryFileManager.AddFileAsync(info, fileName, stream);
                }
            }, null);

        public async Task<MemoryStream?> DownloadFile(string moduleName, string fieldName, string id)
        {
            MemoryStream? mem = null;
            Exception? exp = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var dbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
                    var temporaryFileManager = new TemporaryFileManager(dbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
                    var moduleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), new AuthenticationContext(), dbAccess, temporaryFileManager);

                    var location = moduleDataIO.FileFieldDataIO.GetFileLocation(moduleName!, id!, fieldName!).Result;
                    moduleDataIO.DbAccess.ClearAsync().AsTask().Wait();
                    mem = StorageAccess.ReadFileAsync(location).Result;
                }
                catch (Exception e)
                {
                    exp = e;
                }
            });
            thread.Start();
            //TODO
            //while (thread.IsAlive) Application.DoEvents();
            if (exp != null) await _logger.Error(exp.Message);
            return mem;
        }

        public async Task<MemoryStream?> GetListByExcelFileAsync(SearchCondition condition)
           => await CheckoutException(async dataIO => ExcelUtils.CreateExcelBinary(await dataIO.GetTableTextsAsync(condition!), "data"), null);

        public async Task<List<ModuleSubmitResult>?> SubmitByExcelFileAsync(string moduleName, StreamContent content)
            => await CheckoutException(async dataIO => {
                using (var stream = content.ReadAsStream())
                {
                    var texts = await ExcelUtils.ReadAllTextsFromExcelBinary(stream);
                    if (500 < texts.Count) throw LowCodeException.Create("Excel has a maximum of 500 rows");
                    return await dataIO.SubmitWithTransactionByTableTextsAsync(moduleName, texts);
                }
            }, null);

        public async Task<string> GetCurrentUserIdAsync()
        {
            await Task.CompletedTask;
            return string.Empty;
        }

        async Task<T> CheckoutException<T>(Func<CustomizedModuleDataIO, Task<T>> f, T errResult)
        {
            await using var dbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            var temporaryFileManager = new TemporaryFileManager(dbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
            var dataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), new AuthenticationContext(), dbAccess, temporaryFileManager);
            try
            {
                return await f(dataIO);
            }
            catch (Exception e)
            {
                await _logger.Error(e.Message);
                return errResult;
            }
        }
    }
}
