using Codeer.LowCode.Blazor.SystemSettings;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace LowCodeSamples.Server.Shared
{
  //未解決のDB固有型(NetTypeFullNameがRawDbValueの列)の値変換。DBプロバイダ固有の型変換はここに集約する
  public static class RawDbValueConverter
  {
    //DapperはOracle固有型を知らないため、TypeHandlerでOracleDbTypeを明示してbindする
    class OracleIntervalYMTypeHandler : SqlMapper.TypeHandler<OracleIntervalYM>
    {
      public override void SetValue(IDbDataParameter parameter, OracleIntervalYM value)
      {
        if (parameter is OracleParameter oracleParameter) oracleParameter.OracleDbType = OracleDbType.IntervalYM;
        parameter.Value = value;
      }
      public override OracleIntervalYM Parse(object value) => (OracleIntervalYM)value;
    }

    //Dapperの既定ではbyte[]はDbType.BinaryでbindされOracleはRAW扱い(約32KB上限)になるため、
    //大きいバイナリ(BLOB列のファイル実体等)はOracleDbType.Blobを明示する。
    //2000バイト以下はRAW列にも入れられるよう既定のままにする
    class ByteArrayTypeHandler : SqlMapper.TypeHandler<byte[]>
    {
      public override void SetValue(IDbDataParameter parameter, byte[]? value)
      {
        if (parameter is OracleParameter oracleParameter && value?.Length > 2000) oracleParameter.OracleDbType = OracleDbType.Blob;
        else parameter.DbType = DbType.Binary;
        parameter.Value = (object?)value ?? DBNull.Value;
      }
      public override byte[] Parse(object value) => (byte[])value;
    }

    //PostgreSQLのinterval用ラッパ。TimeSpanのままだとDapperがDbType.Time(=time型)でbindするため型を分ける
    public class NpgsqlIntervalValue
    {
      public TimeSpan Value { get; }
      public NpgsqlIntervalValue(TimeSpan value) => Value = value;
    }

    class NpgsqlIntervalTypeHandler : SqlMapper.TypeHandler<NpgsqlIntervalValue>
    {
      public override void SetValue(IDbDataParameter parameter, NpgsqlIntervalValue? value)
      {
        if (parameter is Npgsql.NpgsqlParameter npgsqlParameter) npgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Interval;
        parameter.Value = (object?)value?.Value ?? DBNull.Value;
      }
      public override NpgsqlIntervalValue Parse(object value) => new((TimeSpan)value);
    }

    static RawDbValueConverter()
    {
      SqlMapper.AddTypeHandler(new OracleIntervalYMTypeHandler());
      SqlMapper.RemoveTypeMap(typeof(byte[]));
      SqlMapper.AddTypeHandler(new ByteArrayTypeHandler());
      SqlMapper.AddTypeHandler(new NpgsqlIntervalTypeHandler());
    }

    //TypeHandlerの登録(staticコンストラクタ)を確実に走らせるための呼び出し口
    internal static void Initialize() { }

    internal static object? ConvertFieldValueToDbValue(DataSourceType? dataSourceType, string rawDbTypeName, object? value)
    {
      if (dataSourceType == DataSourceType.Oracle) return ConvertOracle(rawDbTypeName, value);
      if (dataSourceType == DataSourceType.PostgreSQL) return ConvertPostgreSQL(rawDbTypeName, value);
      return value;
    }

    static object? ConvertPostgreSQL(string rawDbTypeName, object? value)
    {
      switch (rawDbTypeName.ToLowerInvariant())
      {
        case "interval":
          //Dapper既定ではTimeSpanがDbType.Time(=time型)でbindされ型不一致になるため、NpgsqlDbType.Intervalを明示する
          if (value == null) return new NpgsqlIntervalValue(TimeSpan.Zero); //NOT NULL列の既定値
          var timeSpan = value switch
          {
            TimeSpan t => t,
            _ => TimeSpan.Parse(value.ToString() ?? "0:0:0"),
          };
          return new NpgsqlIntervalValue(timeSpan);
      }
      return value;
    }

    static object? ConvertOracle(string rawDbTypeName, object? value)
    {
      var baseType = rawDbTypeName.Split('(')[0].Trim().ToUpperInvariant();
      switch (baseType)
      {
        case "INTERVAL YEAR":
        case "INTERVAL YEAR TO MONTH":
          //読込時にODP.NETが総月数(long)を返すのに合わせ、書込も総月数として解釈する
          var totalMonths = value == null ? 0L : Convert.ToInt64(value);
          return new OracleIntervalYM((int)(totalMonths / 12), (int)(totalMonths % 12));
      }
      return value;
    }
  }
}
