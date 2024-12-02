using AccessSample.Server.Services.AI;
using AccessSample.Server.Services.DataChangeHistory;
using AccessSample.Server.Services.FileManagement;
using Codeer.LowCode.Blazor.SystemSettings;

namespace AccessSample.Server.Services
{
    public class SystemConfig
    {
        public static SystemConfig Instance { get; set; } = new();

        public bool UseHotReload { get; set; }
        public DataSource[] DataSources { get; set; } = [];
        public FileStorage[] FileStorages { get; set; } = [];
        public DataChangeHistoryTableInfo[] DataChangeHistoryTableInfo { get; set; } = [];
        public TemporaryFileTableInfo[] TemporaryFileTableInfo { get; set; } = [];
        public string DesignFileDirectory { get; set; } = string.Empty;
        public string FontFileDirectory { get; set; } = string.Empty;
        public MailSettings MailSettings { get; set; } = new();
        public AISettings AISettings { get; set; } = new();
    }
}
