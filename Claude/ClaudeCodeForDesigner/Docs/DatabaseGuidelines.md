# データベーステーブル作成ガイドライン

Claude Code でテーブルを作成する際の規約。

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
| 日付 | `TEXT` | ISO 8601形式 |
| 日時 | `TEXT` | ISO 8601形式 |
| 真偽値 | `INTEGER` | 0/1 |
