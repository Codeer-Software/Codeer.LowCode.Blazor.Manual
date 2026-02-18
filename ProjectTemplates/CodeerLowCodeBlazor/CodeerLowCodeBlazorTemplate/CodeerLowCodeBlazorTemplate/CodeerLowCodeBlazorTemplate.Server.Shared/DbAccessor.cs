using System.Data;
using System.Data.Common;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.SystemSettings;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace CodeerLowCodeBlazorTemplate.Server.Shared
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

        static DbTableDefinitionCache _dbTableDefinitionCache = new();
        public DbTableDefinitionCache? DbTableDefinitionCache => _dbTableDefinitionCache;
        public static void ClearTableDefinitionCache() => _dbTableDefinitionCache = new();

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
                        conn = new SqlConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.PostgreSQL:
                        conn = new NpgsqlConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.Oracle:
                        conn = new OracleConnection(dataSource.ConnectionString);
                        break;
                    case DataSourceType.SQLite:
                        conn = new SqliteConnection(dataSource.ConnectionString);
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
            var conn = GetConnection(dataSourceName);
            return (await conn.QueryAsync<object>(query, CreateParameter(args), GetTransaction(dataSourceName))).Select(e => (IDictionary<string, object>)e).ToList();
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
    }
}
