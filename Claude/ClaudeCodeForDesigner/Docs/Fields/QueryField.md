QueryField - クエリフィールド
TypeFullName: Codeer.LowCode.Blazor.Repository.Design.QueryFieldDesign
Inherits: FieldDesignBase (implements IDbParameterSettingHolder, IHideDesignForFront)

Purpose: Execute custom SELECT SQL queries. Used instead of the auto-generated SELECT when custom queries are needed. Supports pagination and sorting.

C# class definition (truth source):
```csharp
public class QueryFieldDesign : FieldDesignBase, IDbParameterSettingHolder, IHideDesignForFront
{
    public QuerySetting QuerySetting { get; set; } = new();
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```
QuerySetting / DbParameterSetting の構造は QueryAndSql.md を参照。

Properties:
- QuerySetting (QuerySetting, new()): Query configuration containing:
  - QuerySortType (QuerySortType): None or System. If System, framework adds ORDER BY.
  - QueryPagingType (QueryPagingType): None (all data), System (auto COUNT), CustomCountQuery (user provides COUNT SQL)
  - Parameters (List<DbParameterSetting>): SQLパラメータ定義（入力パラメータと出力列の両方を定義）
    - IsParameter (bool): `true` = SQLの `@name` にバインドされる入力パラメータ / `false` = SELECTの出力列定義
    - Name (string): パラメータ名。入力: SQL内の `@name` と一致。出力: SELECTの列エイリアスおよびフィールドの `DbColumn` と一致。
    - DbType (string): Database type (e.g., "bigint", "varchar", "integer", "DATE")
    - DbParameterDirection (DbParameterDirection): Always Input for QueryField

## 列挙型

### QuerySortType

| 値 | 説明 |
|---|---|
| `None` | ソートなし（SQL側で制御） |
| `System` | フレームワークが ORDER BY を自動追加 |

### QueryPagingType

| 値 | 説明 |
|---|---|
| `None` | ページングなし（全データ取得） |
| `System` | フレームワークが自動で COUNT + LIMIT/OFFSET を追加 |
| `CustomCountQuery` | ユーザーが COUNT SQL を提供 |

SQL File: Stored in separate file named `{ModuleName}.{QueryFieldName}.sql`
- Main query before `--#count.sql` separator
- Optional custom COUNT query after separator
- System reserved parameters: @rows_per_page, @offset (auto-populated for pagination, explicit declaration not required)

## クエリ専用モジュールの構成

QueryFieldを使うモジュールは以下のように構成する:

1. **`DbTable`**: 空文字 `""` にする（実テーブルに対するCRUDは行わない）
2. **`CanCreate`/`CanUpdate`/`CanDelete`**: `false`（読み取り専用）
3. **`Id` フィールド**: 不要
4. **Parameters**: **出力列を `IsParameter: false` で全て定義**し、入力パラメータは `IsParameter: true` で定義
5. **各フィールドの `DbColumn`**: Parameters の `Name` と一致させる
6. **SQLファイル名**: `{ModuleName}.{QueryFieldName}.sql`（QueryField の `Name` プロパティと一致）

## JSON例（集計クエリ）

```json
{
  "QuerySetting": {
    "QuerySortType": "System",
    "QueryPagingType": "System",
    "Parameters": [
      { "IsParameter": false, "Name": "work_date", "DbType": "DATE", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "order_count", "DbType": "INTEGER", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "total_quantity", "DbType": "INTEGER", "DbParameterDirection": "Input" }
    ]
  },
  "Name": "Query",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.QueryFieldDesign"
}
```

## JSON例（検索パラメータ付きクエリ）

```json
{
  "QuerySetting": {
    "QuerySortType": "None",
    "QueryPagingType": "None",
    "Parameters": [
      { "IsParameter": true, "Name": "start_date", "DbType": "Date", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "end_date", "DbType": "Date", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "goods_code", "DbType": "text", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "name", "DbType": "text", "DbParameterDirection": "Input" },
      { "IsParameter": false, "Name": "total_sales_amount", "DbType": "numeric", "DbParameterDirection": "Input" }
    ]
  },
  "Name": "Query",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.QueryFieldDesign"
}
```

> `IsParameter: true`（入力）のパラメータは SQL の `@name` にバインドされる。対応するフィールドに `IsSimpleSearchParameter: true` を設定すると検索条件として利用可能。
> `IsParameter: false`（出力）のパラメータは SELECT の列エイリアスと対応し、各フィールドの `DbColumn` と一致させる。

Runtime: When module has QueryField, system uses this SQL instead of auto-generated SELECT. Parameters bound from module field values. IN clause parameters auto-expanded. Pagination parameters @rows_per_page and @offset auto-set.

No UI rendering (hidden field). No data persistence.

---

## DOM構造（CSS用）

QueryField はUIを持たない（データ取得専用のフィールド）。DOM出力はない。
