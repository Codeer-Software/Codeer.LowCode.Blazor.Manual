using AccessSample.Server.Services.FileManagement;
using AccessSample.Server.Shared;
using Codeer.LowCode.Blazor.DataIO;

namespace AccessSample.Server.Services
{
    public class DataService : IAuthenticationContext, IAsyncDisposable
    {
        public DbAccessor DbAccess { get; }
        public TemporaryFileManager TemporaryFileManager { get; }
        public CustomizedModuleDataIO ModuleDataIO { get; }

        public DataService()
        {
            DbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            TemporaryFileManager = new TemporaryFileManager(DbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
            ModuleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), this, DbAccess, TemporaryFileManager);
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            await Task.CompletedTask;
            return string.Empty;
        }

        public async ValueTask DisposeAsync()
            => await DbAccess.DisposeAsync();
    }
}
