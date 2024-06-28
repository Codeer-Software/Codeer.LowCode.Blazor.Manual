using Codeer.LowCode.Blazor.Repository.Data;

namespace LowCodeSamples.Server.Services.DataChangeHistory
{
  public class ModuleDataChangeHistoryRecord
  {
    public Guid TransactionId { get; set; } = Guid.NewGuid();
    public Guid SubmitId { get; set; } = Guid.NewGuid();

    public ModuleDataChangeType DataChangeType { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string DataId { get; set; } = string.Empty;
    public ModuleData? SubmitData { get; set; }

    public string TableName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
  }
}
