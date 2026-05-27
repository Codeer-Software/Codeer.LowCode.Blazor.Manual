using System.Text;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO.Db;
using Codeer.LowCode.Blazor.SystemSettings;

namespace LowCodeApp.Designer.Lib.AI
{
    public static class DbDefinitionServiceExtensions
    {

        public sealed class DbExtraDefinitions
        {
            public List<DbPackageDefinition> Packages { get; } = new();
            public List<DbRoutineDefinition> Routines { get; } = new();

            //ToString(): 名前のみ
            public override string ToString()
            {
                var sb = new StringBuilder();

                if (Packages.Count > 0)
                {
                    sb.AppendLine("Packages:");
                    foreach (var p in Packages.OrderBy(p => p.QualifiedNameForSort()))
                        sb.AppendLine(p.QualifiedNameForDisplay());
                }

                if (Routines.Count > 0)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendLine("Routines:");
                    foreach (var r in Routines.OrderBy(r => r.QualifiedNameForSort()))
                        sb.AppendLine(r.QualifiedNameForDisplay());
                }

                return sb.ToString().TrimEnd();
            }

            //パラメータ定義付き詳細情報
            public string GetDetailedDefiniations(List<string> names)
            {
                var sb = new StringBuilder();
                sb.AppendLine("# DB Extra Definitions (Detailed)");

                var nameSet = BuildNameSet(names);

                //packages
                var pkgs = Packages
                    .OrderBy(p => p.QualifiedNameForSort())
                    .ToList();

                if (pkgs.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Packages:");
                    foreach (var p in pkgs)
                    {
                        if (!ShouldIncludePackage(p, nameSet)) continue;
                        sb.AppendLine(p.GetDetailedDefiniations(names));
                        sb.AppendLine();
                    }
                }

                //standalone routines
                var routines = Routines
                    .OrderBy(r => r.QualifiedNameForSort())
                    .ToList();

                if (routines.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Routines:");
                    foreach (var r in routines)
                    {
                        if (!ShouldIncludeRoutine(r, nameSet)) continue;
                        sb.AppendLine(r.GetDetailedDefiniations(names));
                        sb.AppendLine();
                    }
                }

                return sb.ToString().TrimEnd();
            }

            static bool ShouldIncludeRoutine(DbRoutineDefinition r, HashSet<string>? nameSet)
            {
                if (nameSet == null || nameSet.Count == 0) return true;

                var q = r.QualifiedNameForSort();
                if (nameSet.Contains(q)) return true;
                if (nameSet.Contains(r.Name)) return true;

                if (!string.IsNullOrWhiteSpace(r.PackageName))
                {
                    var pkgDot = $"{r.PackageName}.{r.Name}";
                    if (nameSet.Contains(pkgDot)) return true;
                }

                return false;
            }

            static bool ShouldIncludePackage(DbPackageDefinition p, HashSet<string>? nameSet)
            {
                if (nameSet == null || nameSet.Count == 0) return true;

                var q = p.QualifiedNameForSort();
                if (nameSet.Contains(q)) return true;
                if (nameSet.Contains(p.Name)) return true;

                foreach (var m in p.Members)
                {
                    if (ShouldIncludeRoutine(m, nameSet)) return true;
                }
                return false;
            }


            static HashSet<string>? BuildNameSet(List<string> names)
            {
                if (names == null || names.Count == 0) return null;
                return new HashSet<string>(
                    names.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        public enum DbRoutineKind { Procedure, Function }
        public enum DbParaDirection { In, Out, InOut, Return, Unknown }

        public sealed class DbRoutineParameter
        {
            public int Ordinal { get; set; }
            public string Name { get; set; } = string.Empty;
            public DbParaDirection Direction { get; set; } = DbParaDirection.Unknown;

            public string DbType { get; set; } = string.Empty;

            public bool IsNullable { get; set; }

            public override string ToString()
            {
                var dir = Direction == DbParaDirection.Unknown ? "" : Direction.ToString().ToUpperInvariant();
                var name = string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name;
                var db = string.IsNullOrWhiteSpace(DbType) ? "?" : DbType;
                var nullable = IsNullable ? " (nullable)" : "";

                if (string.IsNullOrEmpty(dir))
                    return $"- {name}: {db}{nullable}";

                return $"- {dir} {name}: {db}{nullable}";
            }
        }

        public sealed class DbRoutineDefinition
        {
            public string Schema { get; set; } = string.Empty;
            public string PackageName { get; set; } = string.Empty; // Oracle package
            public string Name { get; set; } = string.Empty;

            public DbRoutineKind Kind { get; set; }

            public string ReturnDbType { get; set; } = string.Empty; // function
            public List<DbRoutineParameter> Parameters { get; } = new();

            internal string QualifiedNameForSort()
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Schema)) parts.Add(Schema);
                if (!string.IsNullOrWhiteSpace(PackageName)) parts.Add(PackageName);
                parts.Add(Name);
                return string.Join(".", parts);
            }

            internal string QualifiedNameForDisplay() => QualifiedNameForSort();

            //ToString(): 名前のみ
            public override string ToString() => QualifiedNameForDisplay();

            //詳細情報
            public string GetDetailedDefiniations(List<string> names)
            {
                var sb = new StringBuilder();

                var qname = QualifiedNameForDisplay();

                // 先頭行だけ種別を出す（以降の行には出さない）
                var sig = string.Join(", ",
                    Parameters
                        .Where(p => p.Direction != DbParaDirection.Return)
                        .OrderBy(p => p.Ordinal)
                        .Select(p =>
                        {
                            var pname = string.IsNullOrWhiteSpace(p.Name) ? $"p{p.Ordinal}" : p.Name;
                            var ptype = string.IsNullOrWhiteSpace(p.DbType) ? "?" : p.DbType;
                            var pdir = p.Direction switch
                            {
                                DbParaDirection.In => "IN ",
                                DbParaDirection.Out => "OUT ",
                                DbParaDirection.InOut => "IN OUT ",
                                _ => ""
                            };
                            return $"{pdir}{pname} {ptype}";
                        }));

                if (Kind == DbRoutineKind.Function)
                {
                    var ret = string.IsNullOrWhiteSpace(ReturnDbType) ? "?" : ReturnDbType;
                    sb.AppendLine($"FUNCTION {qname}({sig}) RETURNS {ret}");
                }
                else
                {
                    sb.AppendLine($"PROCEDURE {qname}({sig})");
                }

                if (Parameters.Count > 0)
                {
                    sb.AppendLine("PARAMETERS:");
                    foreach (var p in Parameters.OrderBy(p => p.Ordinal))
                        sb.AppendLine(p.ToString());
                }

                return sb.ToString().TrimEnd();
            }
        }

        public sealed class DbPackageDefinition
        {
            public string Schema { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;

            public List<DbRoutineDefinition> Members { get; } = new();

            internal string QualifiedNameForSort()
                => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

            internal string QualifiedNameForDisplay() => QualifiedNameForSort();

            //ToString(): 名前のみ（種別は付けない）
            public override string ToString() => QualifiedNameForDisplay();

            //詳細情報
            public string GetDetailedDefiniations(List<string> names)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"PACKAGE {QualifiedNameForDisplay()}");

                if (names == null || names.Count == 0)
                {
                    sb.AppendLine("(no package name specified)");
                    return sb.ToString().TrimEnd();
                }

                var nameSet = new HashSet<string>(
                    names.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()),
                    StringComparer.OrdinalIgnoreCase);

                //パッケージ名が合致している場合だけ、members(呼び出せる定義) を含める
                var includeMembers =
                    nameSet.Contains(QualifiedNameForSort()) ||  //SCHEMA.PACKAGE
                    nameSet.Contains(Name);                      //PACKAGE

                if (!includeMembers)
                {
                    sb.AppendLine("(package name not matched)");
                    return sb.ToString().TrimEnd();
                }

                //合致したので callable定義を全件出す
                var members = Members
                    .OrderBy(m => m.Name)
                    .ThenBy(m => m.Kind)
                    .ToList();

                if (members.Count == 0)
                {
                    sb.AppendLine("(no callable members)");
                    return sb.ToString().TrimEnd();
                }

                sb.AppendLine("Members:");
                foreach (var m in members)
                {
                    sb.AppendLine(m.GetDetailedDefiniations(names));
                    sb.AppendLine();
                }

                return sb.ToString().TrimEnd();
            }
        }

        // ============================================================
        // Entry point
        // ============================================================

        public static async Task<DbExtraDefinitions> GetExtraDefinitionsAsync(IDbAccessor access, string dataSourceName)
        {
            switch (access.GetDataSourceType(dataSourceName))
            {
                case DataSourceType.SQLServer:
                    return await GetExtraDefinitionsSqlServerAsync(access, dataSourceName);
                case DataSourceType.PostgreSQL:
                    return await GetExtraDefinitionsPostgreSqlAsync(access, dataSourceName);
                case DataSourceType.Oracle:
                    return await GetExtraDefinitionsOracleAsync(access, dataSourceName);
                case DataSourceType.MySQL:
                    return await GetExtraDefinitionsMySqlAsync(access, dataSourceName);
                case DataSourceType.SQLite:
                    return new DbExtraDefinitions();
                default:
                    throw LowCodeException.Create("InvalidDatasourceType");
            }
        }

        // ============================================================
        // SQL Server
        // ============================================================

        static async Task<DbExtraDefinitions> GetExtraDefinitionsSqlServerAsync(IDbAccessor access, string dataSourceName)
        {
            var result = new DbExtraDefinitions();

            var sql = """
                SELECT
                    r.ROUTINE_SCHEMA AS schema_name,
                    r.ROUTINE_NAME   AS routine_name,
                    r.ROUTINE_TYPE   AS routine_type,
                    r.DATA_TYPE      AS return_data_type,
                    p.ORDINAL_POSITION AS ordinal_position,
                    p.PARAMETER_MODE AS parameter_mode,
                    p.PARAMETER_NAME AS parameter_name,
                    p.DATA_TYPE      AS param_data_type
                FROM INFORMATION_SCHEMA.ROUTINES r
                LEFT JOIN INFORMATION_SCHEMA.PARAMETERS p
                    ON r.SPECIFIC_NAME = p.SPECIFIC_NAME
                   AND r.SPECIFIC_SCHEMA = p.SPECIFIC_SCHEMA
                WHERE r.ROUTINE_TYPE IN ('PROCEDURE','FUNCTION')
                ORDER BY r.ROUTINE_SCHEMA, r.ROUTINE_NAME, p.ORDINAL_POSITION
                """;

            var rows = await access.QueryAsync(dataSourceName, sql, new Dictionary<string, object?>());
            var map = new Dictionary<(string schema, string name, string type), DbRoutineDefinition>(StringTupleComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var schema = GetStr(row, "schema_name");
                var name = GetStr(row, "routine_name");
                var type = GetStr(row, "routine_type");
                var key = (schema, name, type);

                if (!map.TryGetValue(key, out var r))
                {
                    r = new DbRoutineDefinition
                    {
                        Schema = schema,
                        Name = name,
                        Kind = type.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase) ? DbRoutineKind.Procedure : DbRoutineKind.Function,
                        ReturnDbType = GetStr(row, "return_data_type")
                    };
                    map[key] = r;
                }

                var ordinal = GetInt(row, "ordinal_position");
                var pName = GetStr(row, "parameter_name");
                var pMode = GetStr(row, "parameter_mode");
                var pType = GetStr(row, "param_data_type");

                // SQL Server sometimes exposes a return row in INFORMATION_SCHEMA.PARAMETERS for functions
                if (ordinal <= 0 && r.Kind == DbRoutineKind.Function)
                {
                    if (!string.IsNullOrWhiteSpace(pType))
                        r.ReturnDbType = pType;
                    continue;
                }

                if (ordinal <= 0) continue;

                r.Parameters.Add(new DbRoutineParameter
                {
                    Ordinal = ordinal,
                    Name = TrimAtPrefix(pName),
                    Direction = ConvertParameterMode(pMode),
                    DbType = pType,
                    IsNullable = true
                });
            }

            foreach (var r in map.Values)
            {
                result.Routines.Add(r);
            }

            return result;
        }

        // ============================================================
        // PostgreSQL
        // ============================================================

        static async Task<DbExtraDefinitions> GetExtraDefinitionsPostgreSqlAsync(IDbAccessor access, string dataSourceName)
        {
            var result = new DbExtraDefinitions();

            var sql = """
                SELECT
                    r.routine_schema  AS schema_name,
                    r.routine_name    AS routine_name,
                    r.routine_type    AS routine_type,
                    r.data_type       AS return_data_type,
                    p.ordinal_position AS ordinal_position,
                    p.parameter_name  AS parameter_name,
                    p.parameter_mode  AS parameter_mode,
                    p.data_type       AS param_data_type,
                    p.udt_name        AS udt_name
                FROM information_schema.routines r
                LEFT JOIN information_schema.parameters p
                  ON r.specific_name = p.specific_name
                 AND r.specific_schema = p.specific_schema
                WHERE r.routine_schema NOT IN ('pg_catalog','information_schema','pg_toast')
                  AND r.routine_type IN ('FUNCTION','PROCEDURE')
                ORDER BY r.routine_schema, r.routine_name, p.ordinal_position
                """;

            var rows = await access.QueryAsync(dataSourceName, sql, new Dictionary<string, object?>());
            var map = new Dictionary<(string schema, string name, string type), DbRoutineDefinition>(StringTupleComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var schema = GetStr(row, "schema_name");
                if (schema.Equals("public", StringComparison.OrdinalIgnoreCase)) schema = string.Empty;

                var name = GetStr(row, "routine_name");
                var type = GetStr(row, "routine_type");
                var key = (schema, name, type);

                if (!map.TryGetValue(key, out var r))
                {
                    r = new DbRoutineDefinition
                    {
                        Schema = schema,
                        Name = name,
                        Kind = type.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase) ? DbRoutineKind.Procedure : DbRoutineKind.Function,
                        ReturnDbType = GetStr(row, "return_data_type")
                    };
                    map[key] = r;
                }

                var ordinal = GetInt(row, "ordinal_position");
                if (ordinal <= 0) continue;

                var pName = GetStr(row, "parameter_name");
                var pMode = GetStr(row, "parameter_mode");
                var pType = GetStr(row, "param_data_type");
                var udt = GetStr(row, "udt_name");
                if (!string.IsNullOrWhiteSpace(udt) && pType.Equals("USER-DEFINED", StringComparison.OrdinalIgnoreCase))
                    pType = udt;

                r.Parameters.Add(new DbRoutineParameter
                {
                    Ordinal = ordinal,
                    Name = pName,
                    Direction = ConvertParameterMode(pMode),
                    DbType = pType,
                    IsNullable = true
                });
            }

            foreach (var r in map.Values)
            {
                result.Routines.Add(r);
            }

            return result;
        }

        // ============================================================
        // MySQL
        // ============================================================

        static async Task<DbExtraDefinitions> GetExtraDefinitionsMySqlAsync(IDbAccessor access, string dataSourceName)
        {
            var result = new DbExtraDefinitions();

            var db = (await access.QueryAsync(dataSourceName, "SELECT DATABASE() AS DB", new Dictionary<string, object?>()))
                .SelectMany(d => d.Values)
                .FirstOrDefault()?
                .ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(db)) return result;

            var sql = """
                SELECT
                    r.routine_schema AS schema_name,
                    r.routine_name   AS routine_name,
                    r.routine_type   AS routine_type,
                    r.data_type      AS return_data_type,
                    p.ordinal_position AS ordinal_position,
                    p.parameter_name AS parameter_name,
                    p.parameter_mode AS parameter_mode,
                    p.data_type      AS param_data_type,
                    p.dtd_identifier AS dtd_identifier
                FROM information_schema.routines r
                LEFT JOIN information_schema.parameters p
                  ON r.specific_name  = p.specific_name
                 AND r.routine_schema = p.specific_schema
                WHERE r.routine_schema = @p1
                ORDER BY r.routine_name, p.ordinal_position
                """;

            var rows = await access.QueryAsync(dataSourceName, sql, new Dictionary<string, object?> { { "@p1", db } });
            var map = new Dictionary<(string schema, string name, string type), DbRoutineDefinition>(StringTupleComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var schema = GetStr(row, "schema_name");
                var name = GetStr(row, "routine_name");
                var type = GetStr(row, "routine_type");
                var key = (schema, name, type);

                if (!map.TryGetValue(key, out var r))
                {
                    r = new DbRoutineDefinition
                    {
                        Schema = schema,
                        Name = name,
                        Kind = type.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase) ? DbRoutineKind.Procedure : DbRoutineKind.Function,
                        ReturnDbType = GetStr(row, "return_data_type")
                    };
                    map[key] = r;
                }

                var ordinal = GetInt(row, "ordinal_position");
                if (ordinal <= 0) continue;

                var pName = GetStr(row, "parameter_name");
                var pMode = GetStr(row, "parameter_mode");
                var dtd = GetStr(row, "dtd_identifier");
                var pType = !string.IsNullOrWhiteSpace(dtd) ? dtd : GetStr(row, "param_data_type");

                r.Parameters.Add(new DbRoutineParameter
                {
                    Ordinal = ordinal,
                    Name = pName,
                    Direction = ConvertParameterMode(pMode),
                    DbType = pType,
                    IsNullable = true
                });
            }

            foreach (var r in map.Values)
            {
                result.Routines.Add(r);
            }

            return result;
        }

        // ============================================================
        // Oracle
        // ============================================================

        static async Task<DbExtraDefinitions> GetExtraDefinitionsOracleAsync(IDbAccessor access, string dataSourceName)
        {
            var result = new DbExtraDefinitions();

            var owners = (await access.QueryAsync(
                    dataSourceName,
                    "SELECT username FROM all_users WHERE oracle_maintained = 'N'",
                    new Dictionary<string, object?>()))
                .SelectMany(d => d.Values)
                .Select(v => v?.ToString() ?? "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            var defaultUser = (await access.QueryAsync(
                    dataSourceName,
                    "SELECT USER FROM DUAL",
                    new Dictionary<string, object?>()))
                .SelectMany(d => d.Values)
                .FirstOrDefault()?
                .ToString() ?? "";

            foreach (var owner in owners)
            {
                //standalone
                var objs = await access.QueryAsync(
                    dataSourceName,
                    "SELECT OBJECT_NAME, OBJECT_TYPE FROM ALL_OBJECTS WHERE OWNER = :p1 AND OBJECT_TYPE IN ('PROCEDURE','FUNCTION')",
                    new Dictionary<string, object?> { { ":p1", owner } });

                foreach (var row in objs)
                {
                    var objName = row.TryGetValue("OBJECT_NAME", out var n) ? n?.ToString() ?? "" : "";
                    var objType = row.TryGetValue("OBJECT_TYPE", out var t) ? t?.ToString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(objName)) continue;

                    var r = new DbRoutineDefinition
                    {
                        Schema = owner,
                        Name = objName,
                        Kind = objType.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase) ? DbRoutineKind.Function : DbRoutineKind.Procedure
                    };

                    await FillOracleArgumentsAsync(access, dataSourceName, r, owner, null, objName);
                    result.Routines.Add(r);
                }

                //packages
                var pkgs = await access.QueryAsync(
                    dataSourceName,
                    "SELECT OBJECT_NAME FROM ALL_OBJECTS WHERE OWNER = :p1 AND OBJECT_TYPE = 'PACKAGE'",
                    new Dictionary<string, object?> { { ":p1", owner } });

                foreach (var prow in pkgs)
                {
                    var pkgName = prow.TryGetValue("OBJECT_NAME", out var pn) ? pn?.ToString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(pkgName)) continue;

                    var pkg = new DbPackageDefinition { Schema = owner, Name = pkgName };

                    var members = await access.QueryAsync(
                        dataSourceName,
                        "SELECT PROCEDURE_NAME FROM ALL_PROCEDURES WHERE OWNER = :p1 AND OBJECT_NAME = :p2 AND PROCEDURE_NAME IS NOT NULL",
                        new Dictionary<string, object?> { { ":p1", owner }, { ":p2", pkgName } });

                    var memberNames = members
                        .Select(r => r.TryGetValue("PROCEDURE_NAME", out var v) ? v?.ToString() : null)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var m in memberNames)
                    {
                        var r = new DbRoutineDefinition
                        {
                            Schema = owner,
                            PackageName = pkgName,
                            Name = m!,
                            Kind = DbRoutineKind.Procedure
                        };

                        await FillOracleArgumentsAsync(access, dataSourceName, r, owner, pkgName, m!);

                        if (!string.IsNullOrWhiteSpace(r.ReturnDbType))
                            r.Kind = DbRoutineKind.Function;

                        pkg.Members.Add(r);
                    }

                    if (pkg.Members.Count > 0)
                        result.Packages.Add(pkg);
                }
            }

            //デフォルトユーザはスキーマ省略
            if (!string.IsNullOrWhiteSpace(defaultUser))
            {
                foreach (var r in result.Routines)
                    if (r.Schema.Equals(defaultUser, StringComparison.OrdinalIgnoreCase)) r.Schema = "";

                foreach (var p in result.Packages)
                {
                    if (p.Schema.Equals(defaultUser, StringComparison.OrdinalIgnoreCase)) p.Schema = "";
                    foreach (var m in p.Members)
                        if (m.Schema.Equals(defaultUser, StringComparison.OrdinalIgnoreCase)) m.Schema = "";
                }
            }

            return result;
        }

        static async Task FillOracleArgumentsAsync(
            IDbAccessor access,
            string dataSourceName,
            DbRoutineDefinition routine,
            string owner,
            string? packageName,
            string objectName)
        {
            var sql = """
                SELECT
                    POSITION,
                    SEQUENCE,
                    ARGUMENT_NAME,
                    IN_OUT,
                    DATA_TYPE,
                    DATA_LENGTH,
                    DATA_PRECISION,
                    DATA_SCALE
                FROM ALL_ARGUMENTS
                WHERE OWNER = :p1
                  AND OBJECT_NAME = :p2
                  AND (:p3 IS NULL OR PACKAGE_NAME = :p3)
                  AND DATA_LEVEL = 0
                ORDER BY SEQUENCE
                """;

            var rows = await access.QueryAsync(
                dataSourceName,
                sql,
                new Dictionary<string, object?>
                {
                    { ":p1", owner },
                    { ":p2", objectName },
                    { ":p3", packageName }
                });

            foreach (var row in rows)
            {
                var pos = GetInt(row, "POSITION");
                var argName = GetStr(row, "ARGUMENT_NAME");
                var inOut = GetStr(row, "IN_OUT");
                var dataType = GetStr(row, "DATA_TYPE");

                var len = GetNullableInt(row, "DATA_LENGTH");
                var prec = GetNullableInt(row, "DATA_PRECISION");
                var scale = GetNullableInt(row, "DATA_SCALE");

                var dbType = FormatOracleType(dataType, len, prec, scale);

                if (pos == 0)
                {
                    routine.ReturnDbType = dbType;
                    continue;
                }

                if (pos <= 0) continue;

                routine.Parameters.Add(new DbRoutineParameter
                {
                    Ordinal = pos,
                    Name = argName,
                    Direction = ConvertOracleInOut(inOut),
                    DbType = dbType,
                    IsNullable = true
                });
            }
        }

        // ============================================================
        // Helpers
        // ============================================================

        static string GetStr(IDictionary<string, object> row, string key)
            => row.TryGetValue(key, out var v) ? (v?.ToString() ?? "") : "";

        static int GetInt(IDictionary<string, object> row, string key)
        {
            if (!row.TryGetValue(key, out var v) || v == null) return 0;
            if (v is int i) return i;
            return int.TryParse(v.ToString(), out var x) ? x : 0;
        }

        static int? GetNullableInt(IDictionary<string, object> row, string key)
        {
            if (!row.TryGetValue(key, out var v) || v == null) return null;
            if (v is int i) return i;
            return int.TryParse(v.ToString(), out var x) ? x : null;
        }

        static DbParaDirection ConvertParameterMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return DbParaDirection.Unknown;
            switch (mode.Trim().ToUpperInvariant())
            {
                case "IN": return DbParaDirection.In;
                case "OUT": return DbParaDirection.Out;
                case "INOUT":
                case "IN OUT": return DbParaDirection.InOut;
                case "RETURN": return DbParaDirection.Return;
                default: return DbParaDirection.Unknown;
            }
        }

        static DbParaDirection ConvertOracleInOut(string inOut)
        {
            if (string.IsNullOrWhiteSpace(inOut)) return DbParaDirection.Unknown;
            switch (inOut.Trim().ToUpperInvariant())
            {
                case "IN": return DbParaDirection.In;
                case "OUT": return DbParaDirection.Out;
                case "IN/OUT":
                case "IN OUT": return DbParaDirection.InOut;
                default: return DbParaDirection.Unknown;
            }
        }

        static string TrimAtPrefix(string name)
            => string.IsNullOrWhiteSpace(name) ? name : (name.StartsWith("@", StringComparison.Ordinal) ? name.Substring(1) : name);


        static string FormatOracleType(string dataType, int? len, int? prec, int? scale)
        {
            if (string.IsNullOrWhiteSpace(dataType)) return "";

            var t = dataType.Trim().ToUpperInvariant();

            if (t == "VARCHAR2" || t == "CHAR" || t == "NCHAR" || t == "NVARCHAR2")
                return len.HasValue ? $"{t}({len.Value})" : t;

            if (t == "NUMBER")
            {
                if (prec.HasValue && scale.HasValue) return $"NUMBER({prec.Value},{scale.Value})";
                if (prec.HasValue) return $"NUMBER({prec.Value})";
                return "NUMBER";
            }

            return t;
        }

        static DataSourceType GetDataSourceType(this IDbAccessor access, string dataSourceName)
        {
            var dataSource = access.GetDataSource(dataSourceName);
            if (dataSource == null) throw LowCodeException.Create("DataSourceDoesNotExist", dataSourceName);
            return dataSource.DataSourceType;
        }

        static async Task<List<IDictionary<string, object>>> QueryAsync(this IDbAccessor accessor, string dataSourceName, string query, Dictionary<string, object?> args)
            => await accessor.QueryAsync(dataSourceName, query, args.ToDictionary(e => e.Key, e => new ParamAndRawDbTypeName { Value = e.Value }));

        private sealed class StringTupleComparer : IEqualityComparer<(string a, string b, string c)>
        {
            public static readonly StringTupleComparer OrdinalIgnoreCase = new(StringComparer.OrdinalIgnoreCase);
            readonly StringComparer _cmp;
            StringTupleComparer(StringComparer cmp) => _cmp = cmp;

            public bool Equals((string a, string b, string c) x, (string a, string b, string c) y)
                => _cmp.Equals(x.a, y.a) && _cmp.Equals(x.b, y.b) && _cmp.Equals(x.c, y.c);

            public int GetHashCode((string a, string b, string c) obj)
                => HashCode.Combine(
                    _cmp.GetHashCode(obj.a ?? ""),
                    _cmp.GetHashCode(obj.b ?? ""),
                    _cmp.GetHashCode(obj.c ?? ""));
        }
    }
}
