# プロジェクト設定

アプリケーション全体の設定ファイルのリファレンス。プロジェクトファイル、データソース定義、接続文字列の3つのファイルで構成される。

---

## C# クラス定義 (真実の源)

ソースコード `Source/Codeer.LowCode.Blazor/Repository/Design/AppSettingsDesign.cs` 等から転記。`Versions` / `DataSources` / `FileStorages` はすべて `List<...>` (= JSON 配列)。

```csharp
public class AppSettingsDesign
{
    public string CurrentUserModuleDesignName { get; set; } = string.Empty;
    public ModuleMatchCondition AppAccessConditions { get; set; } = new();
    public string BackgroundColor { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }
    public List<VersionInfo> Versions { get; set; } = new();    // 配列
    public string LocalizeResourcePath { get; set; } = string.Empty;
}

public class VersionInfo
{
    public string AssemblyName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public enum DataSourceType { SQLServer, PostgreSQL, Oracle, MySQL, SQLite }

public class DataSource
{
    public string Name { get; set; } = string.Empty;
    public DataSourceType DataSourceType { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    // designer.settings.json (パブリック) には ConnectionString は含めず、
    // designer.settings.Development.json (gitignore 対象) に持つ運用が標準
}
```

> **注意 (Claude 向け)**: `app.clprj` は `AppSettingsDesign` の JSON 直接シリアライズ。`designer.settings.json` の `DataSources` / `FileStorages` は `List<...>` 配列。接続文字列等の機密は `designer.settings.Development.json` (gitignore 対象) に分離する。

---

## 1. app.clprj - プロジェクトファイル

アプリケーション全体の基本設定を定義する。アプリケーションルートに配置。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `CurrentUserModuleDesignName` | string | `""` | ログインユーザーに対応するモジュール名。空の場合は認証なし。 |
| `AppAccessConditions` | ModuleMatchCondition | | アプリケーション全体のアクセス条件 |
| `BackgroundColor` | string | `""` | アプリ全体の背景色 |
| `Color` | string | `""` | アプリ全体の文字色 |
| `FontFamily` | string | `""` | アプリ全体のフォントファミリー |
| `Versions` | List\<VersionInfo\> | `[]` | 使用ライブラリのバージョン情報 |
| `LocalizeResourcePath` | string | `""` | 多言語対応リソースファイルのパス |

### VersionInfo

| プロパティ | 型 | 説明 |
|---|---|---|
| `AssemblyName` | string | アセンブリ名 |
| `Version` | string | バージョン文字列 |

### JSON例

```json
{
  "CurrentUserModuleDesignName": "AppUser",
  "AppAccessConditions": {
    "ModuleName": "",
    "Condition": {
      "IsOrMatch": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "BackgroundColor": "#f8f9fa",
  "Color": "#333333",
  "FontFamily": "'Noto Sans JP', sans-serif",
  "Versions": [
    {
      "AssemblyName": "Codeer.LowCode.Blazor",
      "Version": "1.2.47.0"
    },
    {
      "AssemblyName": "WebApp.Client.Shared",
      "Version": "1.0.0.0"
    }
  ],
  "LocalizeResourcePath": ""
}
```

### 認証なしの最小構成

```json
{
  "CurrentUserModuleDesignName": "",
  "AppAccessConditions": {
    "ModuleName": ""
  }
}
```

---

## 2. designer.settings.json - データソース・ストレージ定義

データベース接続先とファイルストレージの設定。環境に依存しない論理名を定義する。

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `DataSources` | List\<DataSourceSetting\> | データソースの定義リスト |
| `FileStorages` | List\<FileStorageSetting\> | ファイルストレージの定義リスト |
| `FileStorageNames` | List\<string\> | ファイルストレージ名のリスト（旧形式、FileStorages が優先） |

### DataSourceSetting

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | | データソース名。モジュールの `DataSourceName` で参照する。 |
| `DataSourceType` | DataSourceType | | DB種別 |
| `IdentityUserTable` | string | `""` | ASP.NET Identity ユーザーテーブル名（認証使用時） |
| `HasDbContext` | bool | `false` | プロコードで DbContext を使用するか |

### DataSourceType

| 値 | 説明 |
|---|---|
| `PostgreSQL` | PostgreSQL データベース |
| `SQLServer` | Microsoft SQL Server |
| `MySQL` | MySQL データベース |
| `Oracle` | Oracle Database |
| `SQLite` | SQLite データベース |

### FileStorageSetting

| プロパティ | 型 | 説明 |
|---|---|---|
| `Name` | string | ストレージ名。FileField の `StorageName` で参照する。 |
| `FileStorageType` | FileStorageType | ストレージ種別 |

### FileStorageType

| 値 | 説明 |
|---|---|
| `FileSystem` | ローカルファイルシステム |
| `AzureBlobStorage` | Azure Blob Storage |

### JSON例

```json
{
  "DataSources": [
    {
      "Name": "Main",
      "DataSourceType": "PostgreSQL",
      "IdentityUserTable": "",
      "HasDbContext": false
    },
    {
      "Name": "SubDB",
      "DataSourceType": "SQLServer",
      "IdentityUserTable": "",
      "HasDbContext": false
    },
    {
      "Name": "LocalDB",
      "DataSourceType": "SQLite",
      "HasDbContext": false
    }
  ],
  "FileStorages": [
    {
      "FileStorageType": "FileSystem",
      "Name": "Local"
    },
    {
      "FileStorageType": "AzureBlobStorage",
      "Name": "Azure"
    }
  ]
}
```

### 最小構成（SQLite 1データソースのみ）

```json
{
  "DataSources": [
    {
      "Name": "SampleSQLite",
      "DataSourceType": "SQLite"
    }
  ],
  "FileStorageNames": [
    "Local"
  ]
}
```

---

## 3. designer.settings.Development.json - 環境固有設定

接続文字列やデプロイ情報など、環境固有の設定を定義する。`.gitignore` での除外を推奨。

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `ConnectionStrings` | Dictionary\<string, string\> | データソース名 → 接続文字列のマッピング |
| `DeployInfo` | Dictionary\<string, DeployInfoSetting\> | デプロイ先の設定 |
| `CurrentDeployInfoName` | string | 現在使用するデプロイ設定名 |

### DeployInfoSetting

| プロパティ | 型 | 説明 |
|---|---|---|
| `DeployMethod` | string | デプロイ方式: `FileSystem` / `FTPS` |
| `AppName` | string | アプリケーション名 |
| `Directory` | string | デプロイ先ディレクトリ（FileSystem時） |
| `FTPSEndPoint` | string | FTPSエンドポイント（FTPS時） |
| `UserName` | string | 認証ユーザー名（FTPS時） |
| `Password` | string | 認証パスワード（FTPS時） |

### JSON例

```json
{
  "CurrentDeployInfoName": "local",
  "DeployInfo": {
    "local": {
      "DeployMethod": "FileSystem",
      "AppName": "App",
      "Directory": "C:\\Codeer.LowCode.Blazor.Local\\Designs"
    }
  },
  "ConnectionStrings": {
    "Main": "Server=localhost;Database=myapp;Port=5432;Username=postgres;Password=secret;",
    "SubDB": "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=MyDB;User ID=admin;Password=secret;",
    "LocalDB": "Data Source=C:\\Data\\local.db;Version=3;"
  }
}
```

---

## 2ファイルの読み込み・マージの仕組み

`designer.settings.json` と `designer.settings.Development.json` は**どちらもデザインプロジェクト直下**に置かれ、**両方を合わせて初めて「DB接続に必要な情報一式」**になる。接続文字列もデザインプロジェクトの中（`.Development.json`）にある — プロジェクト外の別設定ではない。

**読み込み先クラス:** `DesignerSettings` (`Source/Codeer.LowCode.Blazor.Designer/Config/DesignerSettings.cs`)

```csharp
public class DesignerSettings
{
    public string CurrentDeployInfoName { get; set; } = string.Empty;
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();   // .Development.json
    public Dictionary<string, DeployInfo> DeployInfo { get; set; } = new();      // .Development.json
    public DataSource[] DataSources { get; set; } = [];                          // .json (Name/種別)
    public string[] FileStorageNames { get; set; } = [];                         // .json
}
```

**読み込み手順** (`DesignerViewModel.ReloadConfig()`, 行84-153):

1. `designer.settings.json` を読む。**無い／不正JSONなら空 `DesignerSettings` として扱いエラー記録**（致命的ではない）。
2. `designer.settings.Development.json` を読む。**`FileNotFoundException` は握りつぶす**（無くてOK、`HasDevelopmentConfig = false`）。
3. Development が存在すれば `ObjectMerger.Merge(base, development)` で 2 つを合体する。
4. マージ後、`ConnectionStrings`（**キー=DataSource名 → 値=接続文字列** の辞書）の各エントリを**名前一致**で `DataSources[].ConnectionString` に流し込む。

```csharp
foreach (var e in Config.ConnectionStrings)
{
    var dataSource = Config.DataSources.FirstOrDefault(x => x.Name == e.Key);
    if (dataSource != null) dataSource.ConnectionString = e.Value;
}
```

→ だから `.json` に DataSource の**名前と種別**、`.Development.json` に**同じ名前をキーにした接続文字列**、と分けて書ける。

**`ObjectMerger.Merge()` のマージ規則:**

| 種類 | プロパティ例 | マージ動作 |
|---|---|---|
| 配列 | `DataSources`, `FileStorageNames` | 左（.json）と右（Development）を**連結**（要素追加） |
| 辞書 | `ConnectionStrings`, `DeployInfo` | 左を流し込み → **右でキー重複なら上書き** |
| スカラー | `CurrentDeployInfoName` 等 | **右が非空なら右で上書き** |

## `.Development.json` の取り扱い注意（重要）

- **`.gitignore` で除外されるのが標準**（各プロジェクト直下の `.gitignore` に `designer.settings.Development.json`）。**リポジトリをクローンしただけでは存在しないことがある**。無くても `.json` だけでデザイン定義の読み込み自体は動く（接続文字列が空になるだけ）。
- 中身は**環境ごとに異なり、本番DBの接続文字列を指している可能性もある**。`ConnectionString` を使って実DBへ接続する処理（スキーマ取得・クエリ実行・テーブル作成等）を行う前は、**接続先を確認してから実行する**こと。デザインチェック（headless チェック含む）も DB スキーマ照合のためにこの接続文字列で DB へ接続するため、同じ注意が必要。

---

## DB別の接続文字列例

### PostgreSQL

```
Server=hostname;Database=dbname;Port=5432;Username=user;Password=pass;
```

### SQL Server

```
Server=tcp:hostname,1433;Initial Catalog=dbname;Persist Security Info=False;User ID=user;Password=pass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### MySQL

```
Server=hostname;Database=dbname;Uid=user;Pwd=pass;
```

### Oracle

```
User Id=user;Password=pass;Data Source=hostname:1521/service_name;
```

### SQLite

```
Data Source=C:\\path\\to\\database.db;Version=3;
```

---

## ファイル配置

```
App/
├── app.clprj                           # プロジェクト設定
├── designer.settings.json              # データソース・ストレージ定義
├── designer.settings.Development.json  # 接続文字列・デプロイ設定（環境固有）
├── Modules/                            # モジュール定義
├── PageFrames/                         # ページフレーム定義
└── Resources/                          # リソースファイル
```

## 複数データベースの利用

1つのアプリケーションで複数のデータベースを使用できる。各モジュールの `DataSourceName` で使用するデータソースを指定する。

```json
// designer.settings.json
{
  "DataSources": [
    { "Name": "Main", "DataSourceType": "PostgreSQL" },
    { "Name": "Legacy", "DataSourceType": "SQLServer" },
    { "Name": "Analytics", "DataSourceType": "SQLite" }
  ]
}
```

```json
// Product.mod.json - PostgreSQL を使用
{ "Name": "Product", "DataSourceName": "Main", "DbTable": "product" }

// OldCustomer.mod.json - SQL Server を使用
{ "Name": "OldCustomer", "DataSourceName": "Legacy", "DbTable": "customer" }
```
