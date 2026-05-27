using System.Security.Claims;
using Codeer.LowCode.Blazor.DataIO;
using LowCodeApp.Cookie.Server.Services.FileManagement;
using LowCodeApp.Cookie.Server.Shared;

namespace LowCodeApp.Cookie.Server.Services
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
            TemporaryFileManager = new TemporaryFileManager(DbAccess, SystemConfig.Instance.TemporaryFileTableInfo);
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
