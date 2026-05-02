# プロジェクト設定

アプリケーション全体の設定ファイルのリファレンス。プロジェクトファイル、データソース定義、接続文字列の3つのファイルで構成される。

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
