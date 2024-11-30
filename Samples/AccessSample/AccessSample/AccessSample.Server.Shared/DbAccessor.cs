using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.DataIO.Db.Definition;
using Codeer.LowCode.Blazor.SystemSettings;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;

namespace AccessSample.Server.Shared
{
    public class DbAccessor : IDbAccessor, IDisposable
    {
        bool _transactionMode;

        private class ConnectionOwner
        {
            internal bool NoNeedDispose { get; }
            internal DbConnection Connection { get; }
            internal ConnectionOwner(DbConnection connection, bool noNeedDispose)
            {
                Connection = connection;
                NoNeedDispose = noNeedDispose;
            }
        }

        readonly Dictionary<string, ConnectionOwner> _connections = new();
        readonly Dictionary<string, DbTransaction> _transactions = new();
        readonly Dictionary<string, IDbContextTransaction> _dbContextTransactions = new();
        readonly DataSource[] _dataSources;
        readonly Dictionary<string, DbContext> _dbContexts = new();

        public DbAccessor(DataSource[] dataSources) => _dataSources = dataSources;

        public DbAccessor(DataSource[] dataSources, Dictionary<string, DbContext> dbContext)
        {
            _dataSources = dataSources;
            _dbContexts = dbContext;
        }

        public async Task<List<DbTableDefinition>?> GetCustomTableDefinitionsAsync(string dataSourceName)
        {
            await Task.CompletedTask;

            // Accessの場合、データソース名の特定条件で動作を制御したい場合
            if (!dataSourceName.StartsWith("Access")) return null;

            var conn = GetConnection(dataSourceName) as OleDbConnection;
            if (conn == null) return null;

            // OleDbConnectionでスキーマ情報を取得
#pragma warning disable CA1416 // プラットフォームの互換性を検証
            var columnTable = conn.GetSchema("Columns");
#pragma warning restore CA1416 // プラットフォームの互換性を検証
            if (columnTable == null) return new();

            var ret = columnTable.Rows.Cast<DataRow>()
                .Select(r => new
                {
                    TableName = r["TABLE_NAME"].ToString() ?? "",
                    ColumnName = r["COLUMN_NAME"].ToString() ?? "",
                    RawDbTypeName = r["DATA_TYPE"].ToString() ?? "",
                    IsNullable = (r["IS_NULLABLE"].ToString() ?? "").ToLower() == "yes"
                }).GroupBy(r => r.TableName)
                .Select(g => new DbTableDefinition
                {
                    Name = g.Key,
                    Columns = g.Select(gg => new DbColumnDefinition
                    {
                        Name = gg.ColumnName,
                        RawDbTypeName = gg.RawDbTypeName,
                        NetTypeFullName = ConvertToNetType(gg.RawDbTypeName),
                        IsNullable = gg.IsNullable
                    }).ToList()
                }).ToList();

            return ret;
        }

        static string ConvertToNetType(string? type)
        {
            // Accessでのデータ型を.NET型に変換
            switch (type)
            {
                case "3":  // Long Integer
                    return typeof(int).FullName!;
                case "4":  // Single
                    return typeof(float).FullName!;
                case "5":  // Double
                    return typeof(double).FullName!;
                case "6":  // Currency
                    return typeof(decimal).FullName!;
                case "7":  // Date/Time
                    return typeof(DateTime).FullName!;
                case "11": // Yes/No
                    return typeof(bool).FullName!;
                case "130": // Text
                    return typeof(string).FullName!;
                case "128": // Binary
                    return typeof(byte[]).FullName!;
                case "204": // Memo (OLE object)
                case "205":
                    return typeof(byte[]).FullName!;
                default:
                    return string.Empty;
            }
        }

        public DataSource? GetDataSource(string dataSourceName)
            => _dataSources.FirstOrDefault(e => e.Name == dataSourceName);

        public void StartTransaction()
            => _transactionMode = true;

        public void StartDataAccess(string dataSourceName)
            => GetConnection(dataSourceName);

        public async Task CommitAsync()
        {
            foreach (var e in _transactions)
            {
                await e.Value.CommitAsync();
                await e.Value.DisposeAsync();
            }
            _transactions.Clear();
            foreach (var e in _dbContextTransactions)
            {
                await e.Value.CommitAsync();
                await e.Value.DisposeAsync();
            }
            _dbContextTransactions.Clear();
        }

        public async ValueTask DisposeAsync() => await ClearAsync();

        public async ValueTask ClearAsync()
        {
            foreach (var e in _transactions) await e.Value.DisposeAsync();
            _transactions.Clear();

            foreach (var e in _connections)
            {
                if (e.Value.NoNeedDispose) continue;
                await e.Value.Connection.DisposeAsync();
            }
            _connections.Clear();

            foreach (var e in _dbContextTransactions) await e.Value.DisposeAsync();
            _dbContextTransactions.Clear();
        }

        public void Dispose() => Clear();

        public void Clear()
        {
            foreach (var e in _transactions) e.Value.Dispose();
            _transactions.Clear();

            foreach (var e in _connections)
            {
                if (e.Value.NoNeedDispose) continue;
                e.Value.Connection.Dispose();
            }
            _connections.Clear();

            foreach (var e in _dbContextTransactions) e.Value.Dispose();
            _dbContextTransactions.Clear();
        }

        public DbConnection GetConnection(string dataSourceName)
        {
            if (_connections.TryGetValue(dataSourceName, out var ret)) return ret.Connection;

            var dataSource = _dataSources.FirstOrDefault(e => e.Name == dataSourceName);
            if (dataSource == null)
            {
                throw LowCodeException.Create($"{dataSourceName} not found in ({string.Join(", ", _dataSources.Select(e => e.Name))})");
            }

            if (_dbContexts.TryGetValue(dataSourceName, out var dbContext))
            {
                var conn = dbContext.Database.GetDbConnection();
                conn.Open();
                if (_transactionMode)
                {
                    _dbContextTransactions[dataSourceName] = dbContext!.Database.BeginTransaction();
                }
                _connections.Add(dataSourceName, new ConnectionOwner(conn, true));
                return conn;
            }
            else
            {
                DbConnection conn;
                switch (dataSource.DataSourceType)
                {
                    case DataSourceType.SQLServer:
#pragma warning disable CA1416 // プラットフォームの互換性を検証
                        conn = dataSourceName.StartsWith("Access") ?
                            new OleDbConnection(dataSource.ConnectionString) :
                            new SqlConnection(dataSource.ConnectionString);
#pragma warning restore CA1416 // プラットフォームの互換性を検証
                        break;
                    case DataSourceType.PostgreSQL:
                        conn = new NpgsqlConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.Oracle:
                        conn = new OracleConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.SQLite:
                        conn = new SQLiteConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.MySQL:
                        conn = new MySqlConnection(dataSource.ConnectionString);
                        break;
                    default: throw LowCodeException.Create("Invalid data source");
                }

                conn.Open();
                if (_transactionMode)
                {
                    _transactions[dataSourceName] = conn.BeginTransaction();
                }
                _connections.Add(dataSourceName, new ConnectionOwner(conn, false));
                return conn;
            }
        }

        public IDbTransaction? GetTransaction(string dataSourceName)
        {
            GetConnection(dataSourceName);
            if (_transactions.TryGetValue(dataSourceName, out var transaction)) return transaction;
            if (_dbContextTransactions.TryGetValue(dataSourceName, out var efTransaction)) return efTransaction.GetDbTransaction();
            return null;
        }

        public async Task<int> ExecuteAsync(string dataSourceName, string query, Dictionary<string, object?> args)
        {
            var conn = GetConnection(dataSourceName);
            return await conn.ExecuteAsync(query, CreateParameter(args), GetTransaction(dataSourceName));
        }

        public async Task<string> InsertAsync(string dataSourceName, string query, Dictionary<string, object?> args)
        {
            var conn = GetConnection(dataSourceName);
            var ps = CreateParameter(args);
            var ret = (await conn.ExecuteScalarAsync<string>(query, ps, GetTransaction(dataSourceName))) ?? string.Empty;
            foreach (var e in args)
            {
                args[e.Key] = ps.Get<object>(e.Key);
            }
            return ret;
        }

        public async Task<List<IDictionary<string, object>>> QueryAsync(string dataSourceName, string query, Dictionary<string, ParamAndRawDbTypeName> args)
        {
            //Access対応
            if (dataSourceName.StartsWith("Access"))
            {
                var offset = query.IndexOf("offset ");
                if (offset != -1) query = query.Substring(0, offset);
            }

            var conn = GetConnection(dataSourceName);
            return (await conn.QueryAsync<object>(query, CreateParameter(args), GetTransaction(dataSourceName))).Select(e => (IDictionary<string, object>)e).ToList();
        }

        public virtual Task<string> SubmitIdentityUserAsync(string userId, Dictionary<string, object?> columnAndValue, string? password)
            => throw new NotImplementedException();

        public virtual Task DeleteIdentityUserAsync(string userId)
            => throw new NotImplementedException();

        static Dictionary<string, object?> ToDictionary(IDataRecord record)
        {
            var dictionary = new Dictionary<string, object?>();
            for (int i = 0; i < record.FieldCount; i++)
            {
                dictionary[record.GetName(i)] = record.GetValue(i);
            }
            return dictionary;
        }

        static DynamicParameters CreateParameter(Dictionary<string, ParamAndRawDbTypeName> args)
            => CreateParameter(args.ToDictionary(e => e.Key, e => e.Value.ToParameter()));

        static DynamicParameters CreateParameter(Dictionary<string, object?> args)
        {
            var dst = new DynamicParameters();
            foreach (var e in args)
            {
                var val = e.Value;
                if (val is DateOnly dateOnly) val = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
                if (val is TimeOnly timeOnly) val = new TimeSpan(timeOnly.Hour, timeOnly.Minute, timeOnly.Second);
                dst.Add(e.Key, val);
            }
            return dst;
        }

        static DbType ToDbType(Type? type)
            => type switch
            {
                null => throw new ArgumentNullException(nameof(type)),
                _ when type == typeof(byte) => DbType.Byte,
                _ when type == typeof(sbyte) => DbType.SByte,
                _ when type == typeof(short) => DbType.Int16,
                _ when type == typeof(ushort) => DbType.UInt16,
                _ when type == typeof(int) => DbType.Int32,
                _ when type == typeof(uint) => DbType.UInt32,
                _ when type == typeof(long) => DbType.Int64,
                _ when type == typeof(ulong) => DbType.UInt64,
                _ when type == typeof(float) => DbType.Single,
                _ when type == typeof(double) => DbType.Double,
                _ when type == typeof(decimal) => DbType.Decimal,
                _ when type == typeof(bool) => DbType.Boolean,
                _ when type == typeof(string) => DbType.String,
                _ when type == typeof(char) => DbType.StringFixedLength,
                _ when type == typeof(Guid) => DbType.Guid,
                _ when type == typeof(DateTime) => DbType.DateTime,
                _ when type == typeof(DateTimeOffset) => DbType.DateTimeOffset,
                _ when type == typeof(byte[]) => DbType.Binary,
                _ => DbType.Object
            };
    }
}
