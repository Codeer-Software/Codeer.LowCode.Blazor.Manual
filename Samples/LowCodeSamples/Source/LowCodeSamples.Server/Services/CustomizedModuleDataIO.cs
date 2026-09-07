using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Extras.Services;
using LowCodeSamples.Server.Services.DataChangeHistory;

namespace LowCodeSamples.Server.Services
{
    public class CustomizedModuleDataIO : ModuleDataIO
    {
        //一括INSERT (multi-row INSERT) の有効化。純Addのみの大量Submit (一括取込) がこの行数以上のとき束ねて挿入される。
        //-1 (コア既定) で無効
        static CustomizedModuleDataIO() => BulkAddThreshold = 100;

        readonly DesignData _designData;
        readonly IAuthenticationContext _authenticationContext;
        //サンプル固有: 保存・更新・削除をデータ変更履歴テーブルに記録する (Services/DataChangeHistory)
        readonly DataChangeHistoryService _dataChangeHistory;

        public CustomizedModuleDataIO(DesignData designData, IAuthenticationContext authenticationContext, IDbAccessor dbAccess, ITemporaryFileManager temporaryFileManager)
            : base(designData, authenticationContext, dbAccess, temporaryFileManager)
        {
            _designData = designData;
            _authenticationContext = authenticationContext;
            _dataChangeHistory = new DataChangeHistoryService(dbAccess, SystemConfig.Instance.DataChangeHistoryTableInfo.ToList());
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

            PasswordHashHelper.ApplyPasswordHash(moduleDesign, data);
            var id = await base.AddAsync(transactionId, moduleSubmitId, data);
            await AddHistoryAsync(moduleDesign, transactionId, moduleSubmitId, ModuleDataChangeType.Add, id, data);
            return id;
        }

        //一括INSERT (大量取込) は行ごとの AddAsync を通らないため、同じ加工をこちらでも行う
        protected override async Task BulkAddAsync(Guid transactionId, List<ModuleData> datas)
        {
            var moduleDesign = _designData.Modules.Find(datas.FirstOrDefault()?.Name ?? string.Empty);
            if (moduleDesign == null) throw LowCodeException.Create("invalid design");

            foreach (var data in datas) PasswordHashHelper.ApplyPasswordHash(moduleDesign, data);
            await base.BulkAddAsync(transactionId, datas);
        }

        protected async override Task UpdateAsync(Guid transactionId, Guid moduleSubmitId, ModuleData data)
        {
            var moduleDesign = _designData.Modules.Find(data.Name);
            if (moduleDesign == null) throw LowCodeException.Create("invalid design");

            PasswordHashHelper.ApplyPasswordHash(moduleDesign, data);
            var id = data.Fields.TryGetValue(SystemFieldNames.Id, out var field) ? (field as IdFieldData)?.Value ?? string.Empty : string.Empty;
            await AddHistoryAsync(moduleDesign, transactionId, moduleSubmitId, ModuleDataChangeType.Update, id, data);
            await base.UpdateAsync(transactionId, moduleSubmitId, data);
        }

        protected async override Task DeleteAsync(Guid transactionId, Guid moduleSubmitId, ModuleDeleteInfo moduleDeleteInfo)
        {
            var moduleDesign = _designData.Modules.Find(moduleDeleteInfo.ModuleName);
            if (moduleDesign == null) throw LowCodeException.Create("invalid design");

            await AddHistoryAsync(moduleDesign, transactionId, moduleSubmitId, ModuleDataChangeType.Delete, moduleDeleteInfo.Id, null);
            await base.DeleteAsync(transactionId, moduleSubmitId, moduleDeleteInfo);
        }

        async Task AddHistoryAsync(ModuleDesign moduleDesign, Guid transactionId, Guid moduleSubmitId, ModuleDataChangeType type, string id, ModuleData? data)
            => await _dataChangeHistory.AddDataChangeHistory(moduleDesign.DataSourceName, new ModuleDataChangeHistoryRecord()
            {
                TransactionId = transactionId,
                SubmitId = moduleSubmitId,
                DataChangeType = type,
                ModuleName = moduleDesign.Name,
                DataId = id,
                SubmitData = data,
                TableName = moduleDesign.DbTable,
                UserId = await _authenticationContext.GetCurrentUserIdAsync(),
                DateTime = UtcNowWithoutTimeZone
            });

        //メール送信履歴・承認データなどシステムの記録を、操作ユーザーの書き込み権限に依存せず追加する内部経路。
        //クライアントから直接は呼ばれない (サーバー内部の記録専用)。戻り値は採番された Id
        internal async Task<string> AddSystemRecordAsync(ModuleData data)
            => await AddAsync(Guid.NewGuid(), Guid.NewGuid(), data);

        //承認フローなど、既存レコードへのシステムの記録の書き戻し用内部経路。data に含まれるフィールドだけが更新される
        internal async Task UpdateSystemRecordAsync(ModuleData data)
            => await UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), data);
    }
}
