using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeApp.Cookie.Client.Shared.Services;
using LowCodeApp.Cookie.Server.Services.AI;
using LowCodeApp.Cookie.Server.Services.FileManagement;

namespace LowCodeApp.Cookie.Server.Services
{
    public class PasswordCheckUserTableInfo
    {
        public string TableName { get; set; } = string.Empty;
        public string IdColumn { get; set; } = string.Empty;
        public string UserNameColumn { get; set; } = string.Empty;
        public string HashColumn { get; set; } = string.Empty;
        public string SaltColumn { get; set; } = string.Empty;
    }

    public class SystemConfig
    {
        public static SystemConfig Instance { get; set; } = new();

        public bool CanScriptDebug { get; set; }
        public bool UseHotReload { get; set; }
        public DataSource[] DataSources { get; set; } = [];
        public FileStorage[] FileStorages { get; set; } = [];
        public TemporaryFileTableInfo[] TemporaryFileTableInfo { get; set; } = [];
        public string DesignFileDirectory { get; set; } = string.Empty;
        public string FontFileDirectory { get; set; } = string.Empty;
        public MailSettings MailSettings { get; set; } = new();
        public AISettings AISettings { get; set; } = new();
        public PasswordCheckUserTableInfo PasswordCheckUserTableInfo { get; set; } = new();
        public SystemConfigForFront ForFront() => new SystemConfigForFront { CanScriptDebug = CanScriptDebug, UseHotReload = UseHotReload };
    }
}
