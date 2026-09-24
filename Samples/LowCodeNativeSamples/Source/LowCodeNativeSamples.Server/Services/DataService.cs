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
        readonly IHttpContextAccessor? _httpContextAccessor;
        readonly string? _fixedUserId;

        public DataService(IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
            DbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            TemporaryFileManager = new TemporaryFileManager(DbAccess, SystemConfig.Instance.TemporaryFileTableInfo, FileStorageTable.Storages);
            ModuleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), this, DbAccess, TemporaryFileManager);
        }

        //リクエストの外 (意味検索の再索引などバックグラウンドのジョブ) で、そのユーザーの権限のまま使うための DataService
        public DataService(string userId) : this(httpContextAccessor: null)
            => _fixedUserId = userId;

        public Task<string> GetCurrentUserIdAsync()
            => Task.FromResult(_fixedUserId ?? GetCurrentUserId(_httpContextAccessor?.HttpContext));

        public static string GetCurrentUserId(HttpContext? httpContext)
            => httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        public async ValueTask DisposeAsync()
            => await DbAccess.DisposeAsync();
    }
}
