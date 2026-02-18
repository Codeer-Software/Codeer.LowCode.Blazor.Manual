using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.SystemSettings;

namespace CodeerLowCodeBlazorTemplate.Server.Services.FileManagement
{
    public class TemporaryFileManager : ITemporaryFileManager
    {
        readonly IDbAccessor _dbAccessor;
        readonly TemporaryFileTableInfo[] _temporaryFilesManagements;

        private class SqlParts
        {
            DataSourceType _type;
            public SqlParts(DataSourceType type) => _type = type;
            public string Blanket(string x) => _type == DataSourceType.SQLServer ? $"[{x}]" :
                                                _type == DataSourceType.MySQL ? $"`{x}`" : $"\"{x}\"";
        }

        public TemporaryFileManager(IDbAccessor db, TemporaryFileTableInfo[] temporaryFilesManagements)
        {
            _dbAccessor = db;
            _temporaryFilesManagements = temporaryFilesManagements;
        }

        public async Task ToTemporaryFile(string dataSourceName, Guid guid)
        {
            var dataSource = _dbAccessor.GetDataSource(dataSourceName);
            if (dataSource == null) return;
            var parts = new SqlParts(dataSource.DataSourceType);
            var parameterPrefix = dataSource.DataSourceType == DataSourceType.Oracle ? ":p" : "@p";

            var dateTime = DateTime.UtcNow;
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
            var mgr = _temporaryFilesManagements.FirstOrDefault(e => e.DataSourceName == dataSourceName);
            if (mgr == null) throw LowCodeException.Create($"{dataSourceName} No file management settings");

            //Make it a temporary file and wait for it to be deleted
            var p1 = parameterPrefix + 1;
            var p2 = parameterPrefix + 2;
            var sql = $"insert into {parts.Blanket(mgr.Table)}({parts.Blanket(mgr.GuidColumn)},{parts.Blanket(mgr.CreatedDateTimeColumn)}) values({p1}, {p2})";
            await _dbAccessor.ExecuteAsync(dataSourceName, sql, new Dictionary<string, object?> { { p1, guid }, { p2, dateTime } });
        }

        public async Task FixFile(string dataSourceName, Guid? guid)
        {
            var dataSource = _dbAccessor.GetDataSource(dataSourceName);
            if (dataSource == null) return;
            var parts = new SqlParts(dataSource.DataSourceType);
            var parameterPrefix = dataSource.DataSourceType == DataSourceType.Oracle ? ":p" : "@p";

            var mgr = _temporaryFilesManagements.FirstOrDefault(e => e.DataSourceName == dataSourceName);
            if (mgr == null) throw LowCodeException.Create($"{dataSourceName} No file management settings");
            var p1 = parameterPrefix + 1;
            var sql = $"delete from {parts.Blanket(mgr.Table)} where {parts.Blanket(mgr.GuidColumn)}={p1}";
            await _dbAccessor.ExecuteAsync(dataSourceName, sql, new Dictionary<string, object?> { { p1, guid } });
        }

        public async Task<Codeer.LowCode.Blazor.DataIO.FileInfo> AddFileAsync(FileSaveInfo info, string? fileName, Stream stream)
        {
            var data = new Codeer.LowCode.Blazor.DataIO.FileInfo
            {
                FileName = fileName,
                FileGuid = Guid.NewGuid(),
            };

            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                data.FileSize = memoryStream.Length;
                await WriteTempFile(info.DataSourceName, info.StorageName, data.FileGuid.Value, memoryStream);
            }
            await DeleteTmpFiles(info.DataSourceName, info.StorageName);
            return data;
        }

        async Task DeleteTmpFiles(string dataSourceName, string storageName)
        {
            //You can delete up to 10 items at a time.
            var oldFiles = (await GetOldTemporaryFiles(dataSourceName)).Take(10).ToArray();
            await StorageAccess.DeleteFiles(storageName, oldFiles);
            await RemoveTmpFiles(dataSourceName, oldFiles);
        }

        async Task WriteTempFile(string dataSourceName, string? storageName, Guid guid, MemoryStream memoryStream)
        {
            await ToTemporaryFile(dataSourceName, guid);
            await StorageAccess.WriteFile(storageName, guid, memoryStream);
        }

        async Task<Guid[]> GetOldTemporaryFiles(string dataSourceName)
        {
            var dataSource = _dbAccessor.GetDataSource(dataSourceName);
            if (dataSource == null) return [];
            var parts = new SqlParts(dataSource.DataSourceType);
            var parameterPrefix = dataSource.DataSourceType == DataSourceType.Oracle ? ":p" : "@p";

            var mgr = _temporaryFilesManagements.FirstOrDefault(e => e.DataSourceName == dataSourceName);
            if (mgr == null) throw LowCodeException.Create($"{dataSourceName} No file management settings");

            var old = DateTime.UtcNow.AddDays(-1);
            old = new DateTime(old.Year, old.Month, old.Day, old.Hour, old.Minute, old.Second, old.Millisecond);

            var p1 = parameterPrefix + 1;
            var sql = $"select {parts.Blanket(mgr.GuidColumn)} from {parts.Blanket(mgr.Table)} where {parts.Blanket(mgr.CreatedDateTimeColumn)} < {p1}";
            var list = new List<Guid>();
            foreach (var e in await _dbAccessor.QueryAsync(dataSourceName, sql, new() { { p1, new ParamAndRawDbTypeName { Value = old } } }))
            {
                if (Guid.TryParse(e[mgr.GuidColumn].ToString(), out var guid)) list.Add(guid);
            }
            return list.ToArray();
        }

        async Task RemoveTmpFiles(string dataSourceName, Guid[] oldFiles)
        {
            var dataSource = _dbAccessor.GetDataSource(dataSourceName);
            if (dataSource == null) return;
            var parts = new SqlParts(dataSource.DataSourceType);
            var parameterPrefix = dataSource.DataSourceType == DataSourceType.Oracle ? ":p" : "@p";

            if (!oldFiles.Any()) return;

            var mgr = _temporaryFilesManagements.FirstOrDefault(e => e.DataSourceName == dataSourceName);
            if (mgr == null) throw LowCodeException.Create($"{dataSourceName} No file management settings");

            var dic = oldFiles.Select((e, i) => new { key = $"{parameterPrefix}{i}", value = e }).ToDictionary(e => e.key, e => (object?)e.value);
            var sql = $"delete from {parts.Blanket(mgr.Table)} where {parts.Blanket(mgr.GuidColumn)} in({string.Join(",", dic.Keys)})";
            await _dbAccessor.ExecuteAsync(dataSourceName, sql, dic);
        }
    }
}
