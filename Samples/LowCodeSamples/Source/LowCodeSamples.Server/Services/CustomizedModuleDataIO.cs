using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Data;
using LowCodeSamples.Server.Services.DataChangeHistory;

namespace LowCodeSamples.Server.Services
{
  public class CustomizedModuleDataIO : ModuleDataIO
  {
    readonly DesignData _designData;
    readonly IAuthenticationContext _authenticationContext;
    readonly IDbAccessor _dbAccess;
    readonly DataChangeHistoryService _dataChangeHistory;

    public CustomizedModuleDataIO(DesignData designData, IAuthenticationContext authenticationContext, IDbAccessor dbAccess, ITemporaryFileManager temporaryFileManager)
        : base(designData, authenticationContext, dbAccess, temporaryFileManager)
    {
      _designData = designData;
      _authenticationContext = authenticationContext;
      _dbAccess = dbAccess;
      _dataChangeHistory = new DataChangeHistoryService(_dbAccess, SystemConfig.Instance.DataChangeHistoryTableInfo.ToList());
    }

    static DateTime UtcNowWithoutTimeZone
    {
      get
      {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
      }
    }

    protected override async Task<string> AddAsync(Guid transactionId, Guid moduleSubmitId, ModuleData data)
    {
      var moduleDesign = _designData.Modules.Find(data.Name);
      if (moduleDesign == null) throw LowCodeException.Create("invalid design");

      var id = await base.AddAsync(transactionId, moduleSubmitId, data);
      await _dataChangeHistory.AddDataChangeHistory(moduleDesign.DataSourceName, new ModuleDataChangeHistoryRecord()
      {
        TransactionId = transactionId,
        SubmitId = moduleSubmitId,
        DataChangeType = ModuleDataChangeType.Add,
        ModuleName = moduleDesign.Name,
        DataId = id,
        SubmitData = data,
        TableName = moduleDesign.DbTable,
        UserId = await _authenticationContext.GetCurrentUserIdAsync(),
        DateTime = UtcNowWithoutTimeZone
      });
      return id;
    }

    protected async override Task UpdateAsync(Guid transactionId, Guid moduleSubmitId, ModuleData data)
    {
      var moduleDesign = _designData.Modules.Find(data.Name);
      if (moduleDesign == null) throw LowCodeException.Create("invalid design");

      var id = data.Fields.TryGetValue(SystemFieldNames.Id, out var field) ? (field as IdFieldData)?.Value ?? string.Empty : string.Empty;
      await _dataChangeHistory.AddDataChangeHistory(moduleDesign.DataSourceName, new ModuleDataChangeHistoryRecord()
      {
        TransactionId = transactionId,
        SubmitId = moduleSubmitId,
        DataChangeType = ModuleDataChangeType.Update,
        ModuleName = moduleDesign.Name,
        DataId = id,
        SubmitData = data,
        TableName = moduleDesign.DbTable,
        UserId = await _authenticationContext.GetCurrentUserIdAsync(),
        DateTime = UtcNowWithoutTimeZone
      });
      await base.UpdateAsync(transactionId, moduleSubmitId, data);
    }

    protected async override Task DeleteAsync(Guid transactionId, Guid moduleSubmitId, ModuleDeleteInfo moduleDeleteInfo)
    {
      var moduleDesign = _designData.Modules.Find(moduleDeleteInfo.ModuleName);
      if (moduleDesign == null) throw LowCodeException.Create("invalid design");

      await _dataChangeHistory.AddDataChangeHistory(moduleDesign.DataSourceName, new ModuleDataChangeHistoryRecord()
      {
        TransactionId = transactionId,
        SubmitId = moduleSubmitId,
        DataChangeType = ModuleDataChangeType.Update,
        ModuleName = moduleDesign.Name,
        DataId = moduleDeleteInfo.Id,
        SubmitData = null,
        TableName = moduleDesign.DbTable,
        UserId = await _authenticationContext.GetCurrentUserIdAsync(),
        DateTime = UtcNowWithoutTimeZone
      });
      await base.DeleteAsync(transactionId, moduleSubmitId, moduleDeleteInfo);
    }

    //メール送信履歴などシステムの記録を、操作ユーザーの書き込み権限に依存せず追加する内部経路。
    //クライアントから直接は呼ばれない(サーバー内部の記録専用)。戻り値は採番された Id (承認フローが使う)
    internal async Task<string> AddSystemRecordAsync(ModuleData data)
        => await AddAsync(Guid.NewGuid(), Guid.NewGuid(), data);

    //承認フローなど、既存レコードへのシステムの記録の書き戻し用内部経路。
    //data に含まれるフィールドだけが更新される
    internal async Task UpdateSystemRecordAsync(ModuleData data)
        => await UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), data);
  }
}
