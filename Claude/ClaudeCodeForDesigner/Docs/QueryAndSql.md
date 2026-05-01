# クエリとSQL実行

Codeer.LowCode.Blazor のSQL実行機能のリファレンス。フレームワークは2つのSQL実行モデルを提供する。

---

## 概要

| 機能 | フィールド型 | 用途 | SQL種別 |
|---|---|---|---|
| **QueryField** | QueryFieldDesign | カスタム SELECT | 読み取り専用 |
| **ExecuteSqlField** | ExecuteSqlFieldDesign | INSERT/UPDATE/DELETE/ストアドプロシージャ | 書き込み・実行 |

通常のCRUD操作ではフレームワークが自動的にSQLを生成するため、これらのフィールドは **カスタムSQL が必要な場合にのみ** 使用する。

---

## QueryField - カスタム SELECT クエリ

### 用途

- JOIN を使った複数テーブルの結合クエリ
- サブクエリや集計を含む複雑なSELECT
- フレームワークの自動生成SQLでは対応できない場合

### SQLファイル命名規則

```
{ModuleName}.{QueryFieldName}.sql
```

例: QueryField の `Name` が `Query` の場合 → `ProductSearch.Query.sql`
例: QueryField の `Name` が `クエリ` の場合 → `ProductSearch.クエリ.sql`

### QueryFieldDesign のプロパティ

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.QueryFieldDesign`

| プロパティ | 型 | 説明 |
|---|---|---|
| `QuerySetting` | QuerySetting | クエリ設定 |

#### QuerySetting

| プロパティ | 型 | 説明 |
|---|---|---|
| `QuerySortType` | QuerySortType | `None`: ソートなし / `System`: フレームワークが ORDER BY を追加 |
| `QueryPagingType` | QueryPagingType | `None`: 全件 / `System`: 自動ページング / `CustomCountQuery`: カスタムCOUNT |
| `Parameters` | List\<DbParameterSetting\> | SQLパラメータ定義 |

#### DbParameterSetting

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsParameter` | bool | `true`: SQLの `@name` にバインドされる入力パラメータ / `false`: SELECTの出力列定義 |
| `Name` | string | 入力: SQL内の `@name` と一致させる。出力: SELECTの列エイリアスおよびフィールドの `DbColumn` と一致させる。 |
| `DbType` | string | DB型（例: `"varchar"`, `"bigint"`, `"integer"`, `"DATE"`, `"NUMERIC"`） |
| `DbParameterDirection` | DbParameterDirection | 常に `Input` |

> **重要**: `IsParameter: false` のパラメータで **SELECTの出力列を全て定義** する必要がある。各出力パラメータの `Name` はフィールドの `DbColumn` と一致させる。

### システム予約パラメータ

ページング使用時（`QueryPagingType` が `System`）、以下のパラメータはシステムが自動設定する。明示的な宣言は不要。

| パラメータ名 | 説明 |
|---|---|
| `@rows_per_page` | 1ページあたりの行数 |
| `@offset` | データ取得開始位置 |

### SQLファイルの構造

メインクエリと、オプションのカスタムCOUNTクエリを `--#count.sql` セパレータで区切る。

```sql
-- メインクエリ
SELECT p.id, p.name, p.price, c.name AS category_name
FROM products p
LEFT JOIN categories c ON p.category_id = c.id
WHERE (@category_id = '' OR p.category_id = @category_id)
--#count.sql
-- カスタムCOUNTクエリ（QueryPagingType が CustomCountQuery の場合のみ）
SELECT COUNT(*)
FROM products p
WHERE (@category_id = '' OR p.category_id = @category_id)
```

### クエリ専用モジュールの構成

QueryFieldを使うモジュールは以下のように構成する:

1. **`DbTable`**: 空文字 `""` にする（実テーブルに対するCRUDは行わない）
2. **`CanCreate`/`CanUpdate`/`CanDelete`**: `false`（読み取り専用）
3. **`Id` フィールド**: 不要
4. **Parameters**: 出力列を `IsParameter: false` で全て定義し、入力パラメータは `IsParameter: true` で定義
5. **各フィールドの `DbColumn`**: Parameters の `Name` と一致させる

### 完全なモジュール + SQL の例

#### 例1: 集計クエリ（出力列のみ）

モジュール定義 (`DailySummary.mod.json`):

```json
{
  "Name": "DailySummary",
  "DataSourceName": "Main",
  "DbTable": "",
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "Fields": [
    {
      "QuerySetting": {
        "QuerySortType": "System",
        "QueryPagingType": "System",
        "Parameters": [
          { "IsParameter": false, "Name": "work_date", "DbType": "DATE", "DbParameterDirection": "Input" },
          { "IsParameter": false, "Name": "total_quantity", "DbType": "INTEGER", "DbParameterDirection": "Input" },
          { "IsParameter": false, "Name": "worker_count", "DbType": "INTEGER", "DbParameterDirection": "Input" }
        ]
      },
      "Name": "Query",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.QueryFieldDesign"
    },
    {
      "DbColumn": "work_date",
      "DisplayName": "作業日",
      "Name": "WorkDate",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
    },
    {
      "DbColumn": "total_quantity",
      "DisplayName": "合計数量",
      "Format": "#,##0",
      "Name": "TotalQuantity",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
    },
    {
      "DbColumn": "worker_count",
      "DisplayName": "作業者数",
      "Format": "#,##0",
      "Name": "WorkerCount",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
    }
  ],
  "ListLayouts": {
    "": {
      "Elements": [
        [
          { "FieldName": "WorkDate", "Label": "作業日", "CanResize": true, "CanUserSort": true },
          { "FieldName": "TotalQuantity", "Label": "合計数量", "CanResize": true, "CanUserSort": true },
          { "FieldName": "WorkerCount", "Label": "作業者数", "CanResize": true, "CanUserSort": true }
        ]
      ]
    }
  }
}
```

SQLファイル (`DailySummary.Query.sql`):

```sql
SELECT
  pr.work_date,
  COALESCE(SUM(pr.quantity), 0) AS total_quantity,
  COUNT(DISTINCT pr.worker) AS worker_count
FROM production_results pr
GROUP BY pr.work_date
ORDER BY pr.work_date DESC
```

#### 例2: 検索パラメータ付きクエリ（入力 + 出力）

モジュール定義 (`SalesSummary.mod.json`):

```json
{
  "Name": "SalesSummary",
  "DataSourceName": "Main",
  "DbTable": "",
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "Fields": [
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
    },
    {
      "DbColumn": "start_date",
      "DisplayName": "集計開始日",
      "IsSimpleSearchParameter": true,
      "Name": "StartDate",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
    },
    {
      "DbColumn": "end_date",
      "DisplayName": "集計終了日",
      "IsSimpleSearchParameter": true,
      "Name": "EndDate",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
    },
    {
      "DbColumn": "goods_code",
      "DisplayName": "商品コード",
      "Name": "GoodsCode",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
    },
    {
      "DbColumn": "name",
      "DisplayName": "商品名",
      "Name": "GoodsName",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
    },
    {
      "DbColumn": "total_sales_amount",
      "DisplayName": "合計金額",
      "Format": "#,##0",
      "Name": "TotalSalesAmount",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
    }
  ]
}
```

SQLファイル (`SalesSummary.Query.sql`):

```sql
SELECT
  g.goods_code,
  g.name,
  COALESCE(SUM(sh.amount), 0) AS total_sales_amount
FROM goods AS g
LEFT JOIN sales_history AS sh
  ON sh.goods_code = g.goods_code
 AND sh.sales_date BETWEEN @start_date AND @end_date
GROUP BY g.goods_code, g.name
ORDER BY total_sales_amount DESC
```

> **`IsParameter: true`** の `start_date`, `end_date` は SQL の `@start_date`, `@end_date` にバインドされる。対応フィールドに `IsSimpleSearchParameter: true` を設定すると検索フォームに表示される。
> **`IsParameter: false`** の `goods_code`, `name`, `total_sales_amount` は SELECT の出力列で、各フィールドの `DbColumn` と一致させる。

---

## ExecuteSqlField - SQL実行

### 用途

- カスタム INSERT/UPDATE/DELETE
- ストアドプロシージャの呼び出し
- CRUD操作の前後にカスタムSQLを実行
- トリガー的な処理

### SQLファイル命名規則

```
{ModuleName}.{FieldName}.sql
```

ExecuteSqlField の `Name` プロパティに合わせたファイル名にする。

例:
- フィールド名が `SQL文の実行` の場合: `MyModule.SQL文の実行.sql`
- フィールド名が `ExecuteSql` の場合: `MyModule.ExecuteSql.sql`
- フィールド名が `SQL文の実行リスト前` の場合: `MyModule.SQL文の実行リスト前.sql`

### ExecuteSqlFieldDesign のプロパティ

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign`

| プロパティ | 型 | 説明 |
|---|---|---|
| `Timing` | ExecuteSqlTiming | 実行タイミング |
| `WithStandardIO` | ExecuteSqlWithStandardIO | 標準CRUD処理との実行順序 |
| `ExecuteSqlSetting` | ExecuteSqlSetting | SQL実行設定 |

#### ExecuteSqlTiming

| 値 | 説明 |
|---|---|
| `Standalone` | ボタンクリック等で明示的に実行。Submit処理と独立。 |
| `Create` | レコード作成時に実行 |
| `Update` | レコード更新時に実行 |
| `Delete` | レコード削除時に実行 |

#### ExecuteSqlWithStandardIO

| 値 | 説明 |
|---|---|
| `None` | カスタムSQLのみ実行（標準のINSERT/UPDATE/DELETE処理なし） |
| `Before` | 標準処理を先に実行し、その後カスタムSQL |
| `After` | カスタムSQLを先に実行し、その後標準処理 |

#### Timing と WithStandardIO の組み合わせ

| Timing | WithStandardIO | 実行順序 |
|---|---|---|
| `Standalone` | (無視) | ボタンクリック等で明示的に実行 |
| `Create` | `None` | カスタムSQLのみでINSERT |
| `Create` | `Before` | 標準INSERT → カスタムSQL |
| `Create` | `After` | カスタムSQL → 標準INSERT |
| `Update` | `None` | カスタムSQLのみでUPDATE |
| `Update` | `Before` | 標準UPDATE → カスタムSQL |
| `Update` | `After` | カスタムSQL → 標準UPDATE |
| `Delete` | `None` | カスタムSQLのみでDELETE |
| `Delete` | `Before` | 標準DELETE → カスタムSQL |
| `Delete` | `After` | カスタムSQL → 標準DELETE |

#### ExecuteSqlSetting

| プロパティ | 型 | 説明 |
|---|---|---|
| `CommandType` | ExecuteSqlCommandType | `Sql`: SQL文 / `StoredProcedure`: ストアドプロシージャ |
| `MethodType` | ExecuteSqlMethodType | `NonQuery`: 結果なし / `Reader`: 結果セット / `Scalar`: 単一値 |
| `NewId` | NewId? | INSERT時の新規IDの取得方法 |
| `FailedCondition` | FailedCondition? | 失敗判定条件 |
| `Parameters` | List\<DbParameterSetting\> | SQLパラメータ定義 |

#### DbParameterSetting (ExecuteSql用)

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsParameter` | bool | SQLパラメータとしてバインドするか |
| `Name` | string | パラメータ名 |
| `DbType` | string | DB型（例: `"TEXT"`, `"NUMERIC(18,2)"`, `"integer"`） |
| `DbParameterDirection` | DbParameterDirection | 方向: `Input` / `Output` / `InputOutput` / `ReturnValue` / `MethodReturn` |

#### FailedCondition（失敗判定）

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsMethodReturn` | bool | メソッド戻り値をチェックするか |
| `ParameterName` | string | チェック対象の出力パラメータ名 |
| `CompareType` | FailedConditionCompareType | `Equal`: 値が等しい場合に失敗 / `NotEqual`: 等しくない場合に失敗 |
| `CompareValue` | string | 比較値 |

---

## パラメータバインディング

### DB別パラメータプレフィックス

| DB | プレフィックス | 例 |
|---|---|---|
| PostgreSQL | `@` | `@param_name` |
| SQL Server | `@` | `@param_name` |
| MySQL | `@` | `@param_name` |
| SQLite | `@` | `@param_name` |
| Oracle | `:` | `:param_name` |

### パラメータとフィールドの対応

SQLパラメータ名はモジュールのフィールドの `DbColumn` またはフィールドの `Name` と照合される。パラメータの `Name` がフィールドの `DbColumn` と一致する場合、そのフィールドの値が自動的にバインドされる。

---

## トランザクション処理

- Submit処理内の全SQLは **同一トランザクション** で実行される
- 標準のINSERT/UPDATE/DELETE と ExecuteSqlField のカスタムSQLは同じトランザクション内
- いずれかのSQLが失敗すると **全体がロールバック** される
- Standalone タイミングは独立したトランザクション

---

## 実用例

### 例1: 基本的なINSERT（カスタムSQL）

#### モジュール定義

```json
{
  "Timing": "Create",
  "WithStandardIO": "None",
  "ExecuteSqlSetting": {
    "CommandType": "Sql",
    "MethodType": "NonQuery",
    "Parameters": [
      { "IsParameter": true, "Name": "text_column", "DbType": "TEXT", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "number_column", "DbType": "integer", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "bool_column", "DbType": "boolean", "DbParameterDirection": "Input" },
      { "IsParameter": true, "Name": "date_column", "DbType": "date", "DbParameterDirection": "Input" }
    ]
  },
  "Name": "SQL文の実行",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign"
}
```

#### SQLファイル (`MyModule.SQL文の実行.sql`)

```sql
INSERT INTO my_table (text_column, number_column, bool_column, date_column)
VALUES (
    CONCAT(@text_column, '_custom'),
    @number_column,
    @bool_column,
    @date_column
);
```

### 例2: ストアドプロシージャ（入出力パラメータ）

#### モジュール定義

```json
{
  "Timing": "Standalone",
  "WithStandardIO": "None",
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

#### SQLファイル (`MyModule.ProcCall.sql`)

```sql
CALL my_stored_procedure(
    @p_input_name,
    @p_output_id,
    @p_result
);
```

### 例3: PostgreSQL の INOUT パラメータ

#### SQLファイル

```sql
CALL public.proc_param_test(
    @num_in,           -- IN
    @varchar_in,       -- IN
    @num_inout,        -- INOUT
    @varchar_inout,    -- INOUT
    null,              -- OUT型は参照してはいけない（PostgreSQL制約）
    null               -- OUT
);
```

**PostgreSQL の注意点**: OUT パラメータはCALL文では `null` を渡し、結果はフレームワーク側で取得される。

### 例4: スカラー値の取得

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

```sql
SELECT COUNT(*) FROM products WHERE category_id = @category_id
```

### 例5: 標準INSERT の後にカスタムSQL

レコード作成時、標準INSERTを実行した後に追加のカスタムSQLを実行する。

```json
{
  "Timing": "Create",
  "WithStandardIO": "Before",
  "ExecuteSqlSetting": {
    "CommandType": "Sql",
    "MethodType": "NonQuery",
    "Parameters": [
      { "IsParameter": true, "Name": "id", "DbType": "varchar", "DbParameterDirection": "Input" }
    ]
  },
  "Name": "AfterInsert",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ExecuteSqlFieldDesign"
}
```

```sql
-- 標準INSERTの後に実行される
INSERT INTO audit_log (target_id, action, created_at)
VALUES (@id, 'CREATE', NOW());
```

---

## QueryField vs ExecuteSqlField 比較

| 項目 | QueryField | ExecuteSqlField |
|---|---|---|
| **主な用途** | カスタムSELECT | INSERT/UPDATE/DELETE/ストアド |
| **SQL種別** | SELECT のみ | 任意のSQL |
| **ページング** | あり（System/Custom） | なし |
| **ソート** | あり（System） | なし |
| **パラメータ方向** | Input のみ | Input/Output/InputOutput/ReturnValue |
| **実行タイミング** | データ読み込み時に自動 | Standalone/Create/Update/Delete |
| **トランザクション** | 読み取りのみ | Submit トランザクション内 |
| **ファイル名** | `{Module}.クエリ.sql` | `{Module}.{FieldName}.sql` |
| **UI表示** | なし（非表示フィールド） | なし（非表示フィールド） |
| **TypeFullName** | `...QueryFieldDesign` | `...ExecuteSqlFieldDesign` |

---

## DB別のSQL例

### PostgreSQL

```sql
-- INSERT with RETURNING
INSERT INTO products (name, price) VALUES (@name, @price) RETURNING id;

-- UPSERT
INSERT INTO products (id, name, price)
VALUES (@id, @name, @price)
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name, price = EXCLUDED.price;
```

### SQL Server

```sql
-- INSERT with OUTPUT
INSERT INTO products (name, price)
OUTPUT INSERTED.id
VALUES (@name, @price);

-- MERGE
MERGE INTO products AS target
USING (SELECT @id AS id) AS source
ON target.id = source.id
WHEN MATCHED THEN UPDATE SET name = @name, price = @price
WHEN NOT MATCHED THEN INSERT (name, price) VALUES (@name, @price);
```

### Oracle

```sql
-- パラメータは : プレフィックス
INSERT INTO products (name, price) VALUES (:name, :price);

-- シーケンス使用
INSERT INTO products (id, name, price)
VALUES (product_seq.NEXTVAL, :name, :price);
```
