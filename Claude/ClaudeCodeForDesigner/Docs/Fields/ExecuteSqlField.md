ExecuteSqlField - SQL実行フィールド
TypeFullName: Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign
Inherits: FieldDesignBase (implements IDbParameterSettingHolder, IHideDesignForFront)

Purpose: Execute custom SQL statements or stored procedures. Supports INSERT/UPDATE/DELETE and stored procedures with input/output parameters. Can execute at specific timing during CRUD operations.

Properties:
- Timing (ExecuteSqlTiming): When to execute: Standalone, Create, Update, Delete
- WithStandardIO (ExecuteSqlWithStandardIO): Relationship to standard CRUD: None (custom only), Before (standard first), After (custom first)
- ExecuteSqlSetting (ExecuteSqlSetting): Execution configuration containing:
  - CommandType (ExecuteSqlCommandType): Sql or StoredProcedure
  - MethodType (ExecuteSqlMethodType): NonQuery, Reader, Scalar
  - NewId (NewId?, null): For INSERT - how to get the new record ID
    - IsMethodReturn (bool): Get ID from method return
    - ParameterName (string): Get ID from output parameter
  - FailedCondition (FailedCondition?, new()): How to detect failure
    - IsMethodReturn (bool): Check method return value
    - ParameterName (string): Check output parameter value
    - CompareType (FailedConditionCompareType): Equal or NotEqual
    - CompareValue (string): Value to compare against
  - Parameters (List<DbParameterSetting>): SQL parameter definitions
    - IsParameter (bool): true=SQL parameter, false=just metadata
    - Name (string): Parameter name
    - DbType (string): DB type
    - DbParameterDirection: Input, Output, InputOutput, ReturnValue, MethodReturn

SQL File: Named `{ModuleName}.{FieldName}.sql` (for SQL type) or stored procedure name directly.

## 列挙型

### ExecuteSqlTiming

| 値 | 説明 |
|---|---|
| `Standalone` | ボタンクリック等で明示的に実行 |
| `Create` | レコード作成時に実行 |
| `Update` | レコード更新時に実行 |
| `Delete` | レコード削除時に実行 |

### ExecuteSqlWithStandardIO

| 値 | 説明 |
|---|---|
| `None` | カスタムSQLのみ実行（標準CRUD無し） |
| `Before` | 標準CRUD → カスタムSQL の順に実行 |
| `After` | カスタムSQL → 標準CRUD の順に実行 |

### ExecuteSqlCommandType

| 値 | 説明 |
|---|---|
| `Sql` | SQL文を直接実行 |
| `StoredProcedure` | ストアドプロシージャを実行 |

### ExecuteSqlMethodType

| 値 | 説明 |
|---|---|
| `NonQuery` | 結果セットを返さない（INSERT/UPDATE/DELETE） |
| `Reader` | 結果セットを返す（SELECT） |
| `Scalar` | 単一値を返す |

### DbParameterDirection

| 値 | 説明 |
|---|---|
| `Input` | 入力パラメータ |
| `Output` | 出力パラメータ |
| `InputOutput` | 入出力パラメータ |
| `ReturnValue` | 戻り値 |
| `MethodReturn` | メソッド戻り値 |

### FailedConditionCompareType

| 値 | 説明 |
|---|---|
| `Equal` | 値が等しい場合に失敗 |
| `NotEqual` | 値が等しくない場合に失敗 |

## JSON例

### SQL実行の例

#### 1. 基本的なINSERT
```json
{
  "Timing": "Create",
  "WithStandardIO": "None",
  "ExecuteSqlSetting": {
    "CommandType": "Sql",
    "MethodType": "NonQuery",
    "Parameters": [
      { "IsParameter": true, "Name": "name", "DbType": "varchar", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "price", "DbType": "integer", "DbParameterDirection": "Input" }
    ]
  },
  "Name": "InsertSql",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign"
}
```

SQL file (`Product.InsertSql.sql`):
```sql
INSERT INTO products (name, price) VALUES (@name, @price)
```

#### 2. ストアドプロシージャ（入出力パラメータ）
```json
{
  "Timing": "Create",
  "WithStandardIO": "Before",
  "ExecuteSqlSetting": {
    "CommandType": "StoredProcedure",
    "MethodType": "NonQuery",
    "FailedCondition": {
      "IsMethodReturn": false,
      "ParameterName": "p_result",
      "CompareType": "Equal",
      "CompareValue": "0"
    },
    "Parameters": [
      { "IsParameter": true, "Name": "p_input_name", "DbType": "varchar", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "p_output_id", "DbType": "integer", "DbParameterDirection": "Output" },
      { "IsParameter": true, "Name": "p_result", "DbType": "integer", "DbParameterDirection": "Output" }
    ]
  },
  "Name": "ProcCall",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign"
}
```

#### 3. スカラー値の取得
```json
{
  "Timing": "Standalone",
  "WithStandardIO": "None",
  "ExecuteSqlSetting": {
    "CommandType": "Sql",
    "MethodType": "Scalar",
    "Parameters": [
      { "IsParameter": true, "Name": "category_id", "DbType": "varchar", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "total_count", "DbType": "integer", "DbParameterDirection": "MethodReturn" }
    ]
  },
  "Name": "CountSql",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign"
}
```

SQL file:
```sql
SELECT COUNT(*) FROM products WHERE category_id = @category_id
```

### DB別パラメータ記法
- PostgreSQL/SQL Server/MySQL/SQLite: `@param_name`
- Oracle: `:param_name`

### トランザクション
全SQLはモジュールのSubmit処理内で同一トランザクションで実行される。いずれかのSQLが失敗すると全体がロールバックされる。

### Timing と WithStandardIO の組み合わせ
| Timing | WithStandardIO | 動作 |
|---|---|---|
| Standalone | (無視) | ボタンクリック等で明示的に実行 |
| Create | None | カスタムSQLのみでINSERT |
| Create | Before | 標準INSERT → カスタムSQL |
| Create | After | カスタムSQL → 標準INSERT |
| Update | None | カスタムSQLのみでUPDATE |
| Update | Before | 標準UPDATE → カスタムSQL |
| Delete | None | カスタムSQLのみでDELETE |

No UI rendering (hidden field). No data persistence. Actual execution logic in DataIO layer.

---

## DOM構造（CSS用）

ExecuteSqlField はUIを持たない（SQL実行専用のフィールド）。DOM出力はない。
