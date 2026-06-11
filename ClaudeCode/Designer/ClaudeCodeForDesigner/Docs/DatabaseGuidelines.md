# データベーステーブル作成ガイドライン

Claude Code でテーブルを作成する際の規約。

## DDL の適用・データ確認は sql CLI で

このガイドラインに沿って書いた CREATE TABLE / ALTER などの DDL は、**デザイナ exe の `sql` サブコマンド**でプロジェクトの DB に適用する (CLAUDE.md「SQL 実行 CLI」参照)。自前で DB に接続せず、この CLI で完結させる。

```
"<デザイナexeのパス>" sql "<プロジェクトのルートフォルダ>" --datasource <データソース名> --query "CREATE TABLE ..." --out "<結果JSON>"
```

- DDL の適用、動作確認用のテストデータ投入 (INSERT)、中身の調査 (SELECT: 件数・列・親子の紐付け確認) はすべてこの CLI で行う。`;` 区切りで複数文を一度に流せる (`--file` で .sql ファイルも可)
- 対象データソースの `designer.settings.json` で `AllowCliSqlAccess: true` が必要 (標準テンプレートは設定済み)
- **モジュールを作ったらテーブルも用意する**。テーブル/列が無いまま放置すると designcheck が「列が存在しない」と報告する。モジュール定義と DDL はセットで作り、DDL を sql CLI で適用してから designcheck する

## 主キー (ID)

- **型**: `INTEGER`（long / 64ビット整数）
- **採番**: 自動連番（AUTO INCREMENT）
- **カラム名**: `id`

### SQLite

```sql
CREATE TABLE example (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    ...
);
```

### PostgreSQL

```sql
CREATE TABLE example (
    id BIGSERIAL PRIMARY KEY,
    ...
);
```

### SQL Server

```sql
CREATE TABLE example (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ...
);
```

### MySQL

```sql
CREATE TABLE example (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    ...
);
```

## 外部キー

- 親テーブルの `id` を参照する外部キーは DB 上では `INTEGER` 型にする
- **モジュール定義側**では `NumberFieldDesign` ではなく `LinkFieldDesign` または `IdFieldDesign` を使うこと（フレームワーク内部でテンポラリ ID が文字列として一時的に使われるため）
- DB カラムの型はあくまで実際の ID 型（INTEGER）でよい。テンポラリ ID はアプリ内部の話であり、DB 保存時には実際の ID に変換される

```sql
CREATE TABLE child (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    parent_id INTEGER,
    ...
);
```

## カラム命名規則

- snake_case を使用（例: `report_date`, `total_amount`）
- 外部キーは `{参照先テーブル名の単数形}_id`（例: `expense_report_id`）

## 型の対応

| 用途 | SQLite型 | 備考 |
|---|---|---|
| 主キー | `INTEGER PRIMARY KEY AUTOINCREMENT` | long連番 |
| 外部キー | `INTEGER` | 親テーブルのid参照 |
| テキスト | `TEXT` | |
| 数値（整数） | `INTEGER` | |
| 数値（小数） | `REAL` | |
| 日付 | `DATE` | DateOnly にマップ |
| 日時 | `DATETIME` (または `TIMESTAMP`) | DateTime にマップ |
| 時刻 | `TIME` | TimeOnly にマップ |
| 真偽値 | `INTEGER` | 0/1 |

### SQLite 日付/時刻列は TEXT で宣言しない

SQLite は型なし DB だが、CLB は宣言型 (`PRAGMA table_info` で取得) をフックして .NET 型にマップする (`DbDefinitionServiceSQLite.ConvertToNetType`)。**日付/時刻列を `TEXT` で宣言してはいけない**。

- `DATE` → `DateOnly`
- `DATETIME` / `TIMESTAMP` → `DateTime`
- `TIME` → `TimeOnly`
- `TEXT` → `string` ← これだと CLB が DateOnly を `obj.ToString()` で InvariantCulture (MM/dd/yyyy) 文字列化し、SQLite TEXT 比較が cross-year で破綻する。範囲検索が壊れる

```sql
-- ✅ 正しい
CREATE TABLE Order (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_date DATE,
    updated_at DATETIME,
    start_time TIME,
    ...
);

-- ❌ 範囲検索が壊れる (年跨ぎ等)
CREATE TABLE Order (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_date TEXT,
    updated_at TEXT,
    start_time TEXT,
    ...
);
```

**スキーマ変更後はサーバ再起動が必須**: `DbAccessor._dbTableDefinitionCache` は static で列定義を保持するため、ALTER TABLE / DROP-CREATE しても HotReload では反映されない。
