using Codeer.LowCode.Blazor.DataIO.Db;
using Dapper;

namespace LowCodeSamples.Server.Shared
{
  internal static class ParamAndRawDbTypeNameExtensions
  {
    internal static object? ToParameter(this ParamAndRawDbTypeName param)
    {
      if (string.IsNullOrEmpty(param.RawDbTypeName)) return param.Value;
      if (param.Value is not string text) return param.Value;
      /* 
       * SQL Server / Oracle char/nchar, varchar/nvarchar convert_implicit problem handling
       * With Oracle Managed DataAccess, even if IsAnsi = false, it becomes OracleDbType.Varchar and not NVarchar.
       * If you assign a VARCHAR2 parameter to a NVARCHAR2 column, conversion will be performed using SYS_OP_C2C(:param), but since it applies to the parameter, the index can be used.
       */
      /*
       * Oracle char/nchar does not properly ignore trailing spaces unless you set a fixed length parameter.
       * Blank-padded comparison semantics.
       * https://docs.oracle.com/cd/F19136_01/sqlrf/Data-Type-Comparison-Rules.html#GUID-1563C817-86BF-430B-99AB-322EE2E29187
       */

      var stringFixed = param.IsStringFixedRawDbTypeName();
      // For fixed length characters, use string.length
      return new DbString
      {
        IsAnsi = param.IsUnicodeStringRawDbTypeName(),
        IsFixedLength = stringFixed,
        Value = text,
        Length = stringFixed ? text.Length : -1
      };
    }

    static bool IsUnicodeStringRawDbTypeName(this ParamAndRawDbTypeName p)
    {
      var t = p.RawDbTypeName.ToLower();
      return t == "nchar" || t == "nvarchar" || t == "nvarchar2";
    }

    static bool IsStringFixedRawDbTypeName(this ParamAndRawDbTypeName p)
    {
      var t = p.RawDbTypeName.ToLower();
      return t == "char" || t == "nchar";
    }
  }
}
