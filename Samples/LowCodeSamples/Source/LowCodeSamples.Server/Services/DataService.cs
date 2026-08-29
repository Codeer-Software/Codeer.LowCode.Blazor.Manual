using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;

namespace LowCodeSamples.Server.Services
{
    public class DataService : IAuthenticationContext, IAsyncDisposable
    {
        public DbAccessor DbAccess { get; }
        public TemporaryFileManager TemporaryFileManager { get; }
        public CustomizedModuleDataIO ModuleDataIO { get; }

        public DataService()
        {
            DbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
            TemporaryFileManager = new TemporaryFileManager(DbAccess, SystemConfig.Instance.TemporaryFileTableInfo, SystemConfig.Instance.FileStorages);
            ModuleDataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), this, DbAccess, TemporaryFileManager);
        }

        //デモサイトは認証を持たないため、操作ユーザーは固定 (AppUser の Id=2 「佐藤 花子 (課長)」)。
        //承認フローの「自分の番」判定や CurrentUser 変数に使われる。実運用は Cookie / AAD バリアントのテンプレートを使うこと
        public const string DemoUserId = "2";

        public async Task<string> GetCurrentUserIdAsync()
        {
            await Task.CompletedTask;
            return DemoUserId;
        }

        public async ValueTask DisposeAsync()
            => await DbAccess.DisposeAsync();
    }
}
