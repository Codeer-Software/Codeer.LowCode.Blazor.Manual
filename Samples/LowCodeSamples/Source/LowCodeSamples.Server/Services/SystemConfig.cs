using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeSamples.Client.Shared.Services;
using LowCodeSamples.Server.Services.DataChangeHistory;

namespace LowCodeSamples.Server.Services
{
    public class SystemConfig
    {
        public static SystemConfig Instance { get; set; } = new();

        public bool CanScriptDebug { get; set; }
        public bool CanUpdate { get; set; }
        public bool UseHotReload { get; set; }
        public DataSource[] DataSources { get; set; } = [];
        //保存先は種類ごとの設定を FileStorageTable が IFileStorage に組み立てる
        public List<IFileStorage> FileStorages { get; set; } = [];
        public DataChangeHistoryTableInfo[] DataChangeHistoryTableInfo { get; set; } = [];
        public TemporaryFileTableInfo[] TemporaryFileTableInfo { get; set; } = [];
        public string DesignFileDirectory { get; set; } = string.Empty;
        public string FontFileDirectory { get; set; } = string.Empty;
        public AISettings AISettings { get; set; } = new();
        //Mail = 製品(共通層)が読む設定。プロバイダごとの設定は個別のセクションとして持つ
        public MailConfig Mail { get; set; } = new();
        public SmtpSettings Smtp { get; set; } = new();
        public GraphApiSettings GraphApi { get; set; } = new();
        public GmailSettings Gmail { get; set; } = new();
        //デモ用の固定操作ユーザー (AppUser の Id)。認証を持たないデモサイトで承認フロー等を見せるためのもの
        public string DemoUserId { get; set; } = string.Empty;
        public SystemConfigForFront ForFront() => new SystemConfigForFront { CanScriptDebug = CanScriptDebug, UseHotReload = UseHotReload };
    }
}
