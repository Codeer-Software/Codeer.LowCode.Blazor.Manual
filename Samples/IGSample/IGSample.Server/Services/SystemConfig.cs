using Codeer.LowCode.Blazor.SystemSettings;
using IGSample.Server.Services.DataChangeHistory;
using IGSample.Server.Services.FileManagement;

namespace IGSample.Server.Services
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
  }
}
