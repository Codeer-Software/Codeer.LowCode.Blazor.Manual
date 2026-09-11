using Codeer.LowCode.Blazor.DataIO;
using System.Security.Claims;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.DbAccess;

namespace LowCodeNativeSamples.Server.Services
{
    public class DataService : IAuthenticationContext, IAsyncDisposable
    {
        public DbAccessor DbAccess { get; }
        public TemporaryFileManager TemporaryFileManager { get; }
        public CustomizedModuleDataIO ModuleDataIO { get; }
        readonly IHttpContextAccessor _httpContextAccessor;

        public DataService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            DbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            TemporaryFileManager = new TemporaryFileManager(DbAccess, SystemConfig.Instance.TemporaryFileTableInfo, FileStorageTable.Storages);
            ModuleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), this, DbAccess, TemporaryFileManager);
        }

        public Task<string> GetCurrentUserIdAsync()
            => Task.FromResult(GetCurrentUserId(_httpContextAccessor.HttpContext));

        public static string GetCurrentUserId(HttpContext? httpContext)
            => httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        public async ValueTask DisposeAsync()
            => await DbAccess.DisposeAsync();
    }
}
