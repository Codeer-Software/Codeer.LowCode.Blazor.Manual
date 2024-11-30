using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.SystemSettings;

namespace AccessSample.Server.Services.DataChangeHistory
{
    internal class DataChangeHistoryService
    {
        readonly IDbAccessor _dbAccessor;
        readonly List<DataChangeHistoryTableInfo> _mgr;

        internal DataChangeHistoryService(IDbAccessor dbAccessor, List<DataChangeHistoryTableInfo> mgr)
        {
            _dbAccessor = dbAccessor;
            _mgr = mgr;
        }

        internal async Task AddDataChangeHistory(string ataSourceName, ModuleDataChangeHistoryRecord record)
        {
            var mgr = _mgr.FirstOrDefault(e => e.DataSourceName == ataSourceName);
            if (mgr == null) return;
            if (string.IsNullOrEmpty(mgr.Table)) throw LowCodeException.Create("invalid design");

            var dataSource = _dbAccessor.GetDataSource(mgr.DataSourceName);
            if (dataSource == null) return;

            var parameterPrefix = dataSource.DataSourceType == DataSourceType.Oracle ? ":p" : "@p";
            string Blanket(string x) => $"\"{x}\"";

            var transactionIdParam = parameterPrefix + "transaction";
            var submitIdParam = parameterPrefix + "submit";
            var typeParam = parameterPrefix + "type";
            var moduleParam = parameterPrefix + "module";
            var idParam = parameterPrefix + "id";
            var submitDataParam = parameterPrefix + "submit_data";
            var dataSourceParam = parameterPrefix + "data_source";
            var tableParam = parameterPrefix + "table";
            var userParam = parameterPrefix + "user";
            var nowParam = parameterPrefix + "now";

            var cols = string.Join(",", [
                Blanket(mgr.TransactionIdColumn),
                Blanket(mgr.SubmitIdColumn),
                Blanket(mgr.DataChangeTypeColumn),
                Blanket(mgr.ModuleNameColumn),
                Blanket(mgr.DataIdColumn),
                Blanket(mgr.SubmitDataColumn),
                Blanket(mgr.DataSourceNameColumn),
                Blanket(mgr.TableNameColumn),
                Blanket(mgr.UserIdColumn),
                Blanket(mgr.DateTimeColumn)
            ]);
            var values = string.Join(",", [
                transactionIdParam,
                submitIdParam,
                typeParam,
                moduleParam,
                idParam,
                submitDataParam,
                dataSourceParam,
                tableParam,
                userParam,
                nowParam
            ]);
            var sql = $"insert into {Blanket(mgr.Table)}({cols}) values({values})";
            await _dbAccessor.ExecuteAsync(mgr.DataSourceName, sql,
                new Dictionary<string, object?> {
                    { transactionIdParam, record.TransactionId },
                    { submitIdParam, record.SubmitId },
                    { typeParam, record.DataChangeType.ToString() },
                    { moduleParam,record.ModuleName },
                    { idParam, record.DataId },
                    { submitDataParam, JsonConverterEx.SerializeObject(record.SubmitData) },
                    { dataSourceParam,  mgr.DataSourceName},
                    { tableParam, record.TableName },
                    { userParam, record.UserId },
                    { nowParam, record.DateTime },
                });
        }
    }
}
