# Codeer.LowCode.Blazor - Claude Code によるデザインファイル作成・編集

## 目的

Codeer.LowCode.Blazor のデザインファイル（設定ファイル）を Claude Code で作成・編集する。
デザインファイルはすべてテキストベース（JSON, SQL, C#スクリプト）であるため、
Claude Code が直接読み書きできる。

従来はGUIデザイナー（WPFアプリ）でのみ作成していたが、
Claude Code を活用することで、自然言語の指示からアプリケーション定義を生成可能になる。

## 作業の進め方（着手前に必ず読む）

デザインファイルは「JSON 構文として妥当」でも「業務的に間違い」になりうる。`LimitCount: 0` で明細が 0 件になる、システム項目を編集欄に出す、といった**意味的バグは designcheck では検出できない**（JSON として妥当なため素通りする）。これを着手段階で防ぐためのルール:

1. **規約（Guidelines）が正、JSON 例は参考**。両者が食い違う場合は必ず Guidelines に従う。各 Fields ドキュメントの JSON 例は最小サンプルであり、値が常に最適とは限らない。
2. **着手前に最低限これを読む**: [Docs/CommonMistakes.md](Docs/CommonMistakes.md) / [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) / [Docs/SearchConditionGuidelines.md](Docs/SearchConditionGuidelines.md) / [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) ＋ 作るものに該当する [Docs/AppPatterns/](Docs/AppPatterns/) のパターン。必要箇所だけ grep で拾うのではなく、関連 Guidelines を通読してから書く。
3. **既存をコピーして改変する（ゼロから JSON を組まない）**。
   - 個別のフィールド／レイアウト／検索条件は [Defaults/](Defaults/) の `{型名}.json`（例: `Defaults/DetailListFieldDesign.json`）をコピーし、必要なプロパティだけ上書きする。これはデザイナが新規追加時に書き出すのと**同一の既定状態**（TypeFullName・プロパティ構造・既定値が保証される）なので、`LimitCount` を `0` と書くような“デフォルトでも妥当でもない値”が混入しない。**プロパティの既定値が不確かなときは記憶で書かず Defaults を見る。**
   - モジュール一式は [Samples/PatternShowcase/App/Modules/](Samples/PatternShowcase/App/Modules/) の近いモジュールを正典として複製し、差分だけ直す。
4. **designcheck の緑は「読み込める」までの保証**。0 件表示・編集可能なシステム項目・桁区切りが効かない等の挙動バグは別途、画面（ブラウザ）で確認して潰す（[Docs/BrowserTestGuide.md](Docs/BrowserTestGuide.md)）。
5. **DB を触るときは SQL 実行 CLI**（後述）。テーブル作成（DDL）・テストデータ投入・中身の確認（件数 / 列 / 親子の紐付け）は、自前で DB に接続せず `sql` サブコマンドで行う。

## Codeer.LowCode.Blazor とは

Blazor向け動的ローコードフレームワーク。テキストベースの設定ファイルを読み込んで、
画面・データバインディング・ビジネスロジックを動的に実現するライブラリ。

**動作環境:**
- クライアント: Blazor WebAssembly (WASM)
- サーバー: ASP.NET Core
- デザイナー: WPFアプリ（設定ファイルの作成ツール）

**強み:**
- スクリプト（C#ライクな構文）でビジネスロジックを記述可能
- SQL文を直接実行可能
- すべてテキストベース（JSON + SQL + C#スクリプト）で構成
- コンパイル不要で画面・ロジックを変更可能

**コア仕様のソースコード:** `Source/Codeer.LowCode.Blazor/`

**設定ファイルの実例:**
- [Samples/PatternShowcase/](Samples/PatternShowcase/) - **アプリ作成パターン (認証なし) の全実装サンプル** ([Docs/AppPatterns/](Docs/AppPatterns/) と対応、JSON / SQL / C# テキスト同梱)
- [Samples/PatternShowcaseAuth/](Samples/PatternShowcaseAuth/) - **認証パターン (ログイン / 権限 / 承認ワークフロー) の全実装サンプル**

機能を実装するときは、まず [Docs/AppPatterns/](Docs/AppPatterns/) で該当パターンを確認し、そのパターン名の対応モジュール (各パターン末尾の「標準パターン集の対応」セクション参照) を [Samples/PatternShowcase/App/Modules/](Samples/PatternShowcase/App/Modules/) または [Samples/PatternShowcaseAuth/App/Modules/](Samples/PatternShowcaseAuth/App/Modules/) から開いて、JSON / SQL / C# スクリプトの実物を参考にすること。

## デザインチェック CLI (作成・編集したら必ず実行する)

デザインファイルを作成・編集したら、**デザイナ exe の headless チェックモード**でデザイン定義を検証する。
モジュール一覧から消える・画面が真っ白になるといった「壊れて読み込めない」系の多くを、ブラウザで開く前に検出できる。

### 実行方法

```
"<デザイナexeのパス>" designcheck "<プロジェクトのルートフォルダ>" --out "<結果JSONの出力先パス>"
```

- `designcheck` … サブコマンド
- 第2引数 … `app.clprj` があるプロジェクトのルートフォルダ
- `--out` … 結果を書き出す JSON ファイルのパス (UTF-8)。指定しない場合は何も出力されない (終了コードのみ)
- **終了コード**: `0` = エラーなし / `1` = エラーあり / `2` = 実行失敗 (引数不正・読込失敗・例外)

結果 JSON は読みやすさのため UTF-8・整形済みで出力される:

```json
{
  "project": "...",
  "findingCount": 2,
  "findings": [
    { "type": "PageFrame", "position": "Main", "message": "モジュール 'Foo' のURLセグメント'Foo'は他と重複しています。" }
  ]
}
```

`--out` のパスを読んで `findings` を直し、`findingCount` が 0 (終了コード 0) になるまで繰り返す。

### このチェックが見るもの

- **ファイルの読み込み失敗** — JSON が不正、またはプロパティの型が定義と一致しない (例: 数値プロパティに `"200"` と文字列で書く) と、そのファイルは**静かに無視されてモジュール/ページフレームが欠落**する。これを「ファイル '...' を読み込めませんでした」として検出する (「モジュールが一覧から消えた」系の主因)
- 名前重複、レイアウト構造、スクリプト (文法・参照・型)、モジュール/フィールド/ページフレーム間の参照整合 など、デザイン定義の論理エラー
- **DB スキーマとの照合** — `DbColumn` が実テーブルに存在するか、データソース/テーブルが存在するか。GUI のデザインチェックと同じく、プロジェクト直下の `designer.settings.json` + `designer.settings.Development.json` から接続文字列を読んで **DB に接続し**、スキーマを取得して照合する。**テーブル/列が無いと報告された場合は、後述の「SQL 実行 CLI」で CREATE してから再チェックする**

> **DB に接続する点に注意 (重要)**: このチェックはプロジェクトの接続文字列で DB に接続する。`designer.settings.Development.json` が**本番 DB を指している可能性**もあるため、接続先が不明なプロジェクトに対して実行する前は接続先を確認すること ([Docs/ProjectSettings.md](Docs/ProjectSettings.md) 参照)。DB が起動していない/到達不能なデータソースは「データソースが存在しません」等として報告される (クラッシュはしない)。

### デザイナ exe のパスの調べ方

1. **まず [LocalEnvironment.md](LocalEnvironment.md) を読む** (この CLAUDE.md の隣にあるマシン固有ファイル)。`DesignerExePath:` 行にパスが書いてあればそれを使う
2. 無ければ**ユーザーに聞く**。聞くときは、**起動中のプロセスのうち名前に "Designer" を含むもの**を列挙して候補として提示する (実行中プロセスの実行ファイルパスがそのまま使える)。例 (PowerShell): `Get-Process | Where-Object { $_.Name -like '*Designer*' } | Select-Object Name, Path`
3. パスが分かったら、**[LocalEnvironment.md](LocalEnvironment.md) に書き込む** (次回からは 1 で拾える)。**この CLAUDE.md には書き込まない** — CLAUDE.md は配布されるファイルなので、マシン固有のパスを書くと汚れる。LocalEnvironment.md はマシンローカル (gitignore 対象) で配布物には含まれない

> 補足: デザイナ GUI が起動中でも、別引数 (`designcheck ...`) でもう一つ起動するのは問題ない (別プロセスでプロジェクトを読むだけ。GUI 側には干渉しない)。

## SQL 実行 CLI (DB の中身確認・データ投入・スキーマ変更)

デザイン作業中に DB を直接触りたいとき (明細がちゃんと親に紐づいているか / 件数は / 列の型は / テストデータを入れる / テーブルを作る 等) は、**デザイナ exe の headless SQL モード**で SQL を実行する。`designcheck` と同じ exe・同じ headless 経路で、プロジェクトの接続文字列を使って DB に接続する。

### 実行方法

```
"<デザイナexeのパス>" sql "<プロジェクトのルートフォルダ>" --datasource <データソース名> --query "<SQL>" --out "<結果JSONの出力先パス>"
```

- `sql` … サブコマンド
- 第2引数 … `app.clprj` があるプロジェクトのルートフォルダ
- `--datasource` … `designer.settings.json` の `DataSources` の `Name`
- `--query "<SQL>"` … 実行する SQL。**複数文を `;` 区切りで並べてもよい (全文実行される)**
- `--file "<path>"` … SQL をファイルから読む (`--query` の代わり。migration スクリプト等に)
- `--out "<path>"` … 結果 JSON の出力先 (UTF-8)。省略時は標準出力
- **終了コード**: `0` = 成功 / `2` = 失敗 (引数不正・許可なし・接続/SQL エラー)

### 出力

SELECT 等の結果セットは `results` に列と行、INSERT/UPDATE/DELETE/DDL は `recordsAffected`:

```json
{
  "dataSource": "Main",
  "results": [
    { "columns": ["id", "name"], "rowCount": 2, "rows": [ { "id": 1, "name": "a" }, { "id": 2, "name": "b" } ] }
  ],
  "recordsAffected": -1
}
```

DDL (CREATE / ALTER / DROP) は実行されるが `recordsAffected` は `-1` か `0` になる (行数の概念がないだけで失敗ではない)。

### 許可フラグ (AllowCliSqlAccess)

安全のため、**対象データソースの `designer.settings.json` で `AllowCliSqlAccess: true` のデータソースだけ** SQL を実行できる (既定 `false`)。

```json
"DataSources": [
  { "Name": "Main", "DataSourceType": "SQLite", "AllowCliSqlAccess": true }
]
```

- 標準テンプレート (空 / 入門サンプル / 標準パターン集 / 認証パターン集 / 在庫管理 / SFA / プロジェクト管理) はローカル SQLite なので最初から `true`
- 接続先が不明・本番 DB を指しうるプロジェクトでは `false` のままにして CLI からの実行を防ぐ

> **注意 (重要)**: このコマンドはプロジェクトの接続文字列で DB に**接続して実際に SQL を実行する**。書き込み・削除・スキーマ変更も含み、明示トランザクションを張らない (autocommit) ので即時反映される。読み取り専用モードは無く、実行できる範囲は接続ユーザーの権限次第。`designer.settings.Development.json` が**本番 DB を指している可能性**もあるため、接続先を確認してから使うこと。スキーマを変更した場合、起動中のデザイナ/サーバには列定義のキャッシュのため**再起動するまで反映されない**。

### デザイナ exe のパス

`designcheck` と同じ (上記「デザイナ exe のパスの調べ方」を参照)。GUI 起動中でも別プロセスで実行して問題ない。

## 詳細リファレンス (Docs/)

各設定の詳細なプロパティ、JSON例、ランタイム動作は `Docs/` 以下のドキュメントを参照。

### 全体構成
| ドキュメント | 内容 |
|---|---|
| [Docs/ModuleDesign.md](Docs/ModuleDesign.md) | モジュール定義 (*.mod.json) の全体構造、CRUD権限、権限条件 |
| [Docs/Layouts.md](Docs/Layouts.md) | レイアウト（Detail/List/Search, Grid/Tab/Canvas, FieldLayout） |
| [Docs/PageFrame.md](Docs/PageFrame.md) | ページフレーム (*.frm.json) ナビゲーション構造 |
| [Docs/SearchConditions.md](Docs/SearchConditions.md) | 検索条件 (FieldValueMatch, FieldVariableMatch, MultiMatch) |
| [Docs/QueryAndSql.md](Docs/QueryAndSql.md) | クエリとSQL実行の総合ガイド（QueryField / ExecuteSqlField） |
| [Docs/Scripts.md](Docs/Scripts.md) | C#スクリプト (*.mod.cs) 文法リファレンス、組み込みサービス、Module/Field API |
| [Docs/ScriptExtensions.md](Docs/ScriptExtensions.md) | スクリプト拡張サービス (Excel, WebApi, Toaster, Mail 等) と独自拡張の追加方法 |
| [Docs/ProjectSettings.md](Docs/ProjectSettings.md) | プロジェクト設定 (app.clprj, designer.settings.json) |
| [Docs/Enums.md](Docs/Enums.md) | 全列挙型リファレンス |
| [Docs/AppCss.md](Docs/AppCss.md) | カスタムCSS (app.css) DOM構造・セレクタパターン |
| [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) | レイアウト作成時の推奨ルール（好みに応じて変更可能） |
| [Docs/RowModulePattern.md](Docs/RowModulePattern.md) | 行モジュールパターン: ListField/DetailListField/TileListField で同じ行モジュールを再利用する構成と、AddRowsによるデモデータ準備例 |
| [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) | 一覧ページ・カスタム一覧パターン集 (パターン1〜5: 基本/一括処理/Detail/Tile表示/既定条件/カスタム自前構成)。パターン名でレシピを引ける |
| [Docs/CommonMistakes.md](Docs/CommonMistakes.md) | よくある間違いと対策（JSON型、Elements構造、LinkFieldパス等） |
| [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) | スクリプト作成時の規約・注意事項 |
| [Docs/SearchConditionGuidelines.md](Docs/SearchConditionGuidelines.md) | SearchCondition の LimitCount 設定ガイドライン |
| [Docs/DatabaseGuidelines.md](Docs/DatabaseGuidelines.md) | テーブル作成時の規約（主キー、命名規則、型対応） |
| [Docs/BorderStyleGuide.md](Docs/BorderStyleGuide.md) | カラム枠線（BorderStyle）の設定方法、罫線重複回避ルール、検索グリッドのカード化 |
| [Docs/JsonAbstractTypeFullName.md](Docs/JsonAbstractTypeFullName.md) | JsonAbstract 継承クラスの TypeFullName 一覧・チェックリスト（Field型/Layout型/Match条件/値型） |
| [Docs/BrowserTestGuide.md](Docs/BrowserTestGuide.md) | Playwright によるブラウザ自動スクショで動作確認する手順（settings.local.json への許可追加、WASM 初期化待機、デザイン変更の反映方法） |

### アプリ作成パターン集 (Docs/AppPatterns/)

業務アプリを CLB で組むときに繰り返し登場する「型」を解説する。**機能を実装する前にまず該当パターンを引き、対応する PatternShowcase / PatternShowcaseAuth のモジュールを参考実装として読むこと**。

| ドキュメント | 内容 |
|---|---|
| [Docs/AppPatterns/patterns.md](Docs/AppPatterns/patterns.md) | **全パターンのインデックス**。A〜J の 10 カテゴリ。ここから引く |
| [Docs/AppPatterns/auth_patterns.md](Docs/AppPatterns/auth_patterns.md) | 認証パターン集の入口 (`PatternShowcaseAuth` の対応) |

各パターンの末尾には **「標準パターン集の対応」** セクションがあり、`Samples/PatternShowcase/` (または `Samples/PatternShowcaseAuth/`) のどのモジュールが実装サンプルかを示している。例: 「マスタ参照 (多対1)」パターン → `Samples/PatternShowcase/App/Modules/Shop.mod.json` + `ShopType.mod.json` を読む。

> 注: このディレクトリは [Manual リポジトリ](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual) の `JP/patterns/` と**対になる内容**だが、機械的なコピーではない。Manual は人間向け（解説・画像つき）、こちらは Claude Code 向け（CommonMistakes 参照・JsonAbstract TypeFullName・実装上の注意）で、読者・トーン・リンク先が異なるため、**どちらかを直したらもう一方も読者に合わせて手で反映する**（一括コピーするスクリプトは廃止）。両者で事実（フィールド型・データ構造など）が食い違わないことだけは必ず揃える。

### フィールド型リファレンス (Docs/Fields/)

`Docs/Fields/` ディレクトリ内の全 `.md` ファイルがフィールド型ドキュメント。
フィールド名で該当ファイルを参照すること（例: TextField → `Docs/Fields/TextField.md`）。
外部ライブラリのフィールドも同ディレクトリに含まれる。

**共通ドキュメント（`_` プレフィックス）:**
- [Fields/_FieldCommon.md](Docs/Fields/_FieldCommon.md) - 共通基底プロパティ（FieldDesignBase, ValueFieldDesignBase, DbValueFieldDesignBase, ListFieldDesignBase）
- [Fields/_ScriptApi.md](Docs/Fields/_ScriptApi.md) - フィールド共通スクリプトAPI（全フィールド共通・値フィールド共通のプロパティ/メソッド）

**各フィールド型:** `Docs/Fields/{FieldType}Field.md` の命名規則。外部ライブラリのフィールドも同様。各ファイルにTypeFullName、プロパティ、JSON例、スクリプトAPI、列挙型がすべて自己完結で記載されている。

## デザインファイルの構成

```
App/                                  # アプリケーションルート
├── app.clprj                         # プロジェクト設定 (JSON)
├── app.css                           # カスタムCSS（オプション）
├── designer.settings.json            # データソース・ストレージ定義 (JSON)
├── designer.settings.Development.json # 接続文字列・デプロイ設定 (JSON)
├── Modules/                          # モジュール定義
│   ├── *.mod.json                    # モジュール定義ファイル (JSON)
│   ├── *.mod.cs                      # モジュールのスクリプト (C#)
│   ├── *.{QueryFieldName}.sql         # クエリ用SQL（例: Module.Query.sql）
│   ├── *.{ExecuteSqlFieldName}.sql    # 実行用SQL（例: Module.SQL文の実行.sql）
│   └── サブフォルダ/                   # モジュールをフォルダで整理可能
├── PageFrames/                       # ページフレーム（ナビゲーション）定義
│   └── *.frm.json                    # ページフレーム定義ファイル (JSON)
└── Resources/                        # リソースファイル（画像等）
```

## ファイル形式の詳細

### 1. プロジェクトファイル (`app.clprj`)

アプリケーション全体の設定。

```json
{
  "CurrentUserModuleDesignName": "",
  "AppAccessConditions": { "ModuleName": "" },
  "BackgroundColor": "",
  "Color": "",
  "FontFamily": "",
  "Versions": [
    { "AssemblyName": "Codeer.LowCode.Blazor", "Version": "1.2.47.0" }
  ],
  "LocalizeResourcePath": ""
}
```

### 2. データソース設定 (`designer.settings.json`)

```json
{
  "DataSources": [
    {
      "Name": "Main",
      "DataSourceType": "PostgreSQL",
      "IdentityUserTable": "",
      "HasDbContext": false
    }
  ],
  "FileStorages": [
    { "FileStorageType": "FileSystem", "Name": "Local" }
  ]
}
```

`DataSourceType`: `PostgreSQL` / `SQLServer` / `MySQL` / `Oracle` / `SQLite`

### 3. モジュール定義ファイル (`*.mod.json`)

モジュール = 画面（ページ/フォーム）の定義。最も重要なファイル。

#### モジュールの2つの用途

1. **CRUDモジュール**: DBテーブルとマッピングし、データの作成・読み取り・更新・削除を行う。`DbTable` にテーブル名を指定し、`IdFieldDesign`、`SubmitButtonFieldDesign`、`ListLayouts` を設定する。
2. **表示専用モジュール**: DBと結びつかず、チャートダッシュボードやダイアログ等の表示のみを行う。この場合:
   - `DbTable` は空文字 `""` にする
   - `DataSourceName` は空文字 `""` にする
   - `IdFieldDesign` は不要（定義しない）
   - `SubmitButtonFieldDesign` は不要（定義しない）
   - `ListLayouts` の Elements にフィールドを入れないこと。フィールドを入れると一覧表示モジュールと認識されてしまう

#### 基本構造

```json
{
  "Name": "モジュール名",
  "DataSourceName": "データソース名",
  "DbTable": "DBテーブル名",
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "Fields": [ /* フィールド定義の配列 */ ],
  "UserWriteCondition": { "ModuleName": "" },
  "UserReadCondition": { "ModuleName": "" },
  "DataWriteCondition": { "ModuleName": "" },
  "DataReadCondition": { "ModuleName": "" },
  "DetailLayouts": { /* 詳細画面レイアウト */ },
  "ListLayouts": { /* 一覧画面レイアウト */ },
  "SearchLayouts": { /* 検索画面レイアウト */ },
  "LinkFieldNames": [],
  "ListPageFieldDesign": { /* 一覧ページ設定 */ }
}
```

#### フィールド定義

各フィールドは `TypeFullName` で型を指定する。主要な型は以下:

**IdFieldDesign** - 主キー
```json
{
  "DbColumn": "id",
  "IsManualInput": false,
  "CompositeIdVariables": [],
  "CompositeIdSeparator": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Id",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
}
```

**TextFieldDesign** - テキスト入力
```json
{
  "DbColumn": "name",
  "IsMultiline": false,
  "IsAutoFitRows": false,
  "Placeholder": "",
  "MaxLength": null,
  "Rows": null,
  "TextEditEmptyType": "StringEmpty",
  "ShouldTrimAfterEdit": false,
  "SearchComparisonDefaultValue": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Name",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

**NumberFieldDesign** - 数値入力
```json
{
  "DbColumn": "price",
  "Placeholder": "",
  "Format": "",
  "Min": null,
  "Max": null,
  "IsSlider": false,
  "Step": null,
  "MaxFractionDigits": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "DisplayName": "",
  "IsRequired": false,
  "Name": "Price",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

**BooleanFieldDesign** - チェックボックス/トグル
```json
{
  "DbColumn": "is_active",
  "Text": "有効",
  "UIType": "CheckBox",
  "TrueText": "",
  "FalseText": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "IsActive",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```
`UIType`: `CheckBox` / `ToggleButton` / `Switch`

**DateFieldDesign** - 日付入力
```json
{
  "DbColumn": "created_date",
  "Format": "",
  "IsYearMonthOnly": false,
  "IsUpdateProtected": false,
  "Name": "CreatedDate",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
}
```

**DateTimeFieldDesign** - 日時入力
```json
{
  "DbColumn": "updated_at",
  "SaveAsUtc": false,
  "Format": "",
  "IsUpdateProtected": false,
  "Name": "UpdatedAt",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign"
}
```

**TimeFieldDesign** - 時刻入力
```json
{
  "DbColumn": "start_time",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "Name": "StartTime",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign"
}
```

**SelectFieldDesign** - ドロップダウン選択
```json
{
  "DbColumn": "status",
  "Candidates": ["未着手,1", "進行中,2", "完了,3"],
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "",
    "Condition": { "IsOrMatch": false, "IsNot": false, "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition" }
  },
  "ValueVariable": "",
  "DisplayTextVariable": "",
  "EmptyCandidateType": "StringEmpty",
  "AllowOrSearch": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Status",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign"
}
```
`Candidates`: `"表示テキスト,値"` の形式。値省略時は表示テキスト=値。
別モジュールから候補を取得する場合は `SearchCondition.ModuleName` + `ValueVariable` + `DisplayTextVariable` を設定。

**RadioGroupFieldDesign** + **RadioButtonFieldDesign** - ラジオボタン
```json
{
  "DbColumn": "priority",
  "AllowOrSearch": false,
  "PopulateRadioButtons": false,
  "Name": "Priority",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioGroupFieldDesign"
}
```
各ボタン:
```json
{
  "Text": "高",
  "Value": "High",
  "GroupField": "Priority",
  "Name": "PriorityHigh",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
}
```

**FileFieldDesign** - ファイルアップロード
```json
{
  "StorageName": "Local",
  "DbColumnFileName": "file_name",
  "DbColumnFileSize": "file_size",
  "DbColumnFileGuid": "file_guid",
  "ObjectFit": "Contain",
  "ShowPreview": false,
  "IsUpdateProtected": false,
  "DisplayName": "",
  "Name": "Attachment",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign"
}
```

**PasswordFieldDesign** - パスワード入力
```json
{
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Password",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.PasswordFieldDesign"
}
```

**LabelFieldDesign** - ラベル（表示のみ）
```json
{
  "Text": "タイトル",
  "Icon": "",
  "Style": "H1",
  "RelativeField": "",
  "OnClick": "",
  "Name": "TitleLabel",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```
`Style`: `Default` / `H1` / `H2` / `H3` / `H4` / `H5` / `H6`
`RelativeField`: 指定すると、そのフィールドの値を表示テキストとして使用。

**ImageViewerFieldDesign** - 画像表示
```json
{
  "ResourcePath": "lc_logo_256.png",
  "ObjectFit": "Contain",
  "OnClick": "",
  "Name": "Logo",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ImageViewerFieldDesign"
}
```

**ButtonFieldDesign** - ボタン
```json
{
  "Text": "保存",
  "Icon": "",
  "Variant": "Primary",
  "OnClick": "SaveButton_OnClick",
  "ImageResourcePath": "",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "ShowTextInToolTip": false,
  "Name": "SaveButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
}
```
`Variant`: `Primary` / `Secondary` / `Success` / `Danger` / `Warning` / `Info` / `Light` / `Dark` / `Link`

**SubmitButtonFieldDesign** - 送信ボタン（データ保存）
```json
{
  "Text": "登録",
  "Icon": "",
  "Variant": "Primary",
  "IsBlock": true,
  "ImageResourcePath": "",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "Name": "SubmitButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign"
}
```

**LinkFieldDesign** - 他モジュールとのリンク（外部キー）
```json
{
  "DbColumn": "category_id",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Category",
    "Condition": { "IsOrMatch": false, "IsNot": false, "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition" }
  },
  "ValueVariable": "Id.Value",
  "DisplayTextVariable": "Name.Value",
  "ListLayoutName": "",
  "SearchLayoutName": "",
  "OnSearchButtonClicked": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Category",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
}
```
- `DbColumn`: 自テーブルの外部キー列
- `SearchCondition.ModuleName`: 参照先モジュール名
- `ValueVariable`: 参照先のID取得式（例: `Id.Value`）
- `DisplayTextVariable`: 参照先の表示テキスト取得式（例: `Name.Value`）

**ListFieldDesign** - 一覧表示（子レコード表示等）
```json
{
  "LayoutName": "",
  "CanNavigateToDetail": true,
  "NavigateModuleUrlSegment": "",
  "CanCustomizeColumns": false,
  "DisplayName": "",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "ModuleName": "OrderItem",
    "Condition": {
      "IsOrMatch": false, "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "OrderId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Bottom",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "CanUserSort": true,
  "CanSelect": false,
  "Name": "Items",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
}
```

**DetailListFieldDesign** - 明細リスト（親子関係のインライン編集）
```json
{
  "LayoutName": "",
  "DisplayName": "",
  "SearchCondition": {
    "ModuleName": "OrderDetail",
    "Condition": { ... }
  },
  "DeleteTogether": true,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "Name": "Details",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
}
```

**SearchFieldDesign** - 検索フォーム
```json
{
  "ResultsViewFieldName": "List",
  "LayoutName": "",
  "OnSearched": "",
  "UserUrlParameter": false,
  "SearchInitializationTriggerUrlParameter": "",
  "Name": "Search",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign"
}
```

**ModuleFieldDesign** - 他モジュールの埋め込み
```json
{
  "DbColumn": "",
  "ModuleName": "EmbeddedModule",
  "LayoutName": "",
  "IsUpdateProtected": false,
  "IgnoreModification": false,
  "Name": "EmbeddedForm",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign"
}
```

**AnchorTagFieldDesign** - リンク/ナビゲーション
```json
{
  "Style": "Text",
  "Target": "Url",
  "ShouldOpenInNewTab": false,
  "Icon": "",
  "TitleText": "詳細を見る",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "",
  "Module": "TargetModule",
  "ModuleVariable": "",
  "IdVariable": "Id.Value",
  "Url": "",
  "OnClick": "",
  "Name": "DetailLink",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```
`Target`: `Url`（URLまたはモジュールへのナビゲーション） / `HistoryBack`（ブラウザ戻る） / `HistoryForward`（ブラウザ進む）
モジュール遷移は `Target: "Url"` + `Module` プロパティで実現する。

**HeaderMenuFieldDesign** / **SidebarMenuFieldDesign** / **ContextMenuFieldDesign** - メニュー系
```json
{
  "LayoutName": "",
  "PageFrame": "",
  "Name": "HeaderMenu",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

#### 詳細画面レイアウト (DetailLayouts)

GridLayoutDesign でフィールドを配置する。`""` キーがデフォルトレイアウト。

```json
"DetailLayouts": {
  "": {
    "OnBeforeInitialization": "",
    "OnAfterInitialization": "",
    "OnLocationChanging": "",
    "OnFieldDataChanged": "",
    "DataOnlyFields": [],
    "ClassName": "",
    "Color": "",
    "BackgroundColor": "",
    "Layout": {
      "Name": "",
      "Padding": {},
      "IsBordered": false,
      "IsFlowLayout": false,
      "IsFillAvailable": false,
      "ScrollDirection": "Unset",
      "BackgroundColor": "",
      "Rows": [
        {
          "IsWrap": false,
          "Margin": {},
          "GridRowType": "Normal",
          "CanResize": false,
          "BackgroundColor": "",
          "Columns": [
            {
              "Layout": {
                "FieldName": "NameLabel",
                "ContextMenu": "",
                "ClassName": "",
                "FontFamily": "",
                "Color": "",
                "BackgroundColor": "",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": 150,
              "Padding": {},
              "BackgroundColor": "",
              "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
              "VerticalAlignment": "Middle",
              "HorizontalAlignment": "Left",
              "CanResize": false,
              "Border": "None"
            },
            {
              "Layout": {
                "FieldName": "Name",
                "ContextMenu": "",
                "ClassName": "",
                "FontFamily": "",
                "Color": "",
                "BackgroundColor": "",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": null,
              "Padding": {},
              "BackgroundColor": "",
              "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
              "VerticalAlignment": "Middle",
              "CanResize": false,
              "Border": "None"
            }
          ]
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    }
  }
}
```

**レイアウト構造:**
- `GridLayoutDesign` → `Rows[]` → `Columns[]` → `FieldLayoutDesign`（フィールド名を指定）
- `Width: null` は自動幅、数値指定で固定幅（px）
- `VerticalAlignment`: `Top` / `Middle` / `Bottom`
- `HorizontalAlignment`: `Left` / `Center` / `Right`

**ネスト可能なレイアウト:**
- `GridLayoutDesign` - グリッド（行・列ベース）
- `TabLayoutDesign` - タブ切り替え
- `CanvasLayoutDesign` - 絶対位置配置

#### 一覧画面レイアウト (ListLayouts)

```json
"ListLayouts": {
  "": {
    "HeaderTitle": "",
    "DataOnlyFields": [],
    "OnBeforeInitialization": "",
    "OnAfterInitialization": "",
    "OnFieldDataChanged": "",
    "Elements": [
      [
        {
          "FieldName": "Code",
          "Label": "コード",
          "ColumnSpan": 1,
          "RowSpan": 1,
          "TextWrap": "Unset",
          "CanResize": true,
          "CanUserSort": true,
          "ClassName": "",
          "Color": "",
          "BackgroundColor": "",
          "DetailLayoutName": ""
        },
        {
          "FieldName": "Name",
          "Label": "名前",
          "ColumnSpan": 1,
          "RowSpan": 1,
          "CanResize": true,
          "CanUserSort": true
        }
      ]
    ]
  }
}
```

`Elements` は行の配列（通常1行）。各行内の配列が横に並ぶ列を定義する。

> レイアウト作成時の推奨ルールは [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) を参照。

#### 検索画面レイアウト (SearchLayouts)

```json
"SearchLayouts": {
  "": {
    "OnSearchInitialization": "",
    "ShowDefaultSearchButtons": true,
    "Layout": {
      "Operator": "And",
      "Rows": [ /* GridLayoutDesign と同じ構造 */ ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

### 4. ページフレーム定義ファイル (`*.frm.json`)

アプリのナビゲーション構造を定義する。

```json
{
  "IsApplicationRoot": true,
  "Name": "Main",
  "Description": "",
  "Left": {
    "IsVisible": true,
    "Home": { "Type": "Text", "Text": "Home", "Icon": "", "ResourcePath": "" },
    "Links": [
      {
        "Title": "商品一覧",
        "Icon": "",
        "IconType": "Font",
        "HideTitle": false,
        "ModulePageType": "Auto",
        "ModuleUrlSegment": "",
        "ActiveModuleSegments": [],
        "PageFrame": "",
        "Module": "Product",
        "Id": "",
        "Parameters": "",
        "ListPageDesign": {
          "SearchLayoutName": "",
          "UserUrlParameter": true,
          "PageTitle": "",
          "HeaderTitle": "",
          "CanBulkDataUpdate": false,
          "CanBulkDataDownload": false,
          "UseSubmitButton": false,
          "UseNavigateToCreate": true,
          "ListFieldDesign": {
            "LayoutName": "",
            "CanNavigateToDetail": true,
            "NavigateModuleUrlSegment": "",
            "CanCustomizeColumns": false,
            "DisplayName": "",
            "SearchCondition": {
              "LimitCount": 50,
              "SelectFields": [],
              "SortConditions": [],
              "SortFieldVariable": "",
              "SortDescending": false,
              "ModuleName": ""
            },
            "PagerPosition": "Bottom",
            "UseIndexSort": false,
            "DeleteTogether": false,
            "CanCreate": false,
            "CanUpdate": false,
            "CanDelete": true,
            "CanUserSort": true,
            "CanSelect": false,
            "Name": "",
            "IgnoreModification": false,
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
          }
        },
        "DetailPageDesign": {
          "PageTitle": "",
          "LayoutName": "",
          "UrlParameter": ""
        }
      }
    ]
  },
  "Right": { "IsVisible": false, "Links": [] },
  "Header": { "IsVisible": false, "Links": [] }
}
```

- `ModulePageType`: `Auto`（自動判定）/ `ListToDetail`（一覧→詳細）/ `List`（一覧のみ）/ `Detail`（詳細のみ）

### 5. スクリプトファイル (`*.mod.cs`)

モジュールのイベントハンドラをC#ライクな構文で記述。
ファイル名はモジュール名と一致させる（例: `Product.mod.cs`）。

```csharp
// フィールドのイベントハンドラ
void SaveButton_OnClick()
{
    // フィールドに直接アクセス可能
    if (Name.Value == "")
    {
        MessageBox.Show("名前を入力してください");
        return;
    }
    LabelResult.Text = "保存しました";
}

void Status_OnDataChanged()
{
    // フィールドの値変更時の処理
    LabelResult.Text = "ステータスが変更されました: " + Status.Value;
}

void Search_OnSearchDataChanged()
{
    // 検索条件変更時の処理
}
```

**スクリプトで利用可能なオブジェクト:**
- モジュール内の全フィールド（フィールド名で直接アクセス）
- `MessageBox.Show(message)` - メッセージ表示
- `Logger.Log(message)` / `Logger.Warn(message)` / `Logger.Error(message)`
- `NavigationService` - ページナビゲーション
- `Resources` - リソースアクセス
- `BatchSearcher` - 複数モジュール一括検索

**イベントの命名規則:**
- `{フィールド名}_OnClick` - ボタンクリック時
- `{フィールド名}_OnDataChanged` - 値変更時
- `{フィールド名}_OnSearchDataChanged` - 検索条件変更時
- `OnBeforeInitialization` - 初期化前（DetailLayout/ListLayoutで定義）
- `OnAfterInitialization` - 初期化後
- `OnLocationChanging` - ページ離脱前
- `OnFieldDataChanged` - いずれかのフィールド変更時

### 6. SQLファイル

モジュールに関連するSQLクエリ。ファイル名規則:
- `{モジュール名}.{QueryFieldのName}.sql` - SELECT用クエリ（例: `Summary.Query.sql`）
- `{モジュール名}.{ExecuteSqlFieldのName}.sql` - INSERT/UPDATE/DELETE等の実行SQL

パラメータは `@パラメータ名` で記述。モジュールのフィールド値がバインドされる。

**QueryField を使うクエリ専用モジュールの構成:**
- `DbTable`: 空文字 `""` にする
- `CanCreate`/`CanUpdate`/`CanDelete`: `false`
- `Id` フィールド: 不要
- QuerySetting.Parameters: **出力列を `IsParameter: false`** で全て定義、入力パラメータは `IsParameter: true` で定義
- 各フィールドの `DbColumn`: Parameters の `Name` と一致させる
- システムページングパラメータ（`rows_per_page`, `offset`）は明示的宣言不要

## 検索条件の構造 (MatchCondition)

モジュール間のデータ関連付けや検索フィルタに使用。

**FieldVariableMatchCondition** - フィールド同士の比較
```json
{
  "SearchTargetVariable": "CategoryId.Value",
  "Comparison": "Equal",
  "Variable": "Id.Value",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
}
```

**FieldValueMatchCondition** - フィールドと固定値の比較
```json
{
  "SearchTargetVariable": "Status.Value",
  "Comparison": "Equal",
  "Value": { "Value": "Active" },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
}
```

**MultiMatchCondition** - 複合条件（AND/OR）
```json
{
  "IsOrMatch": false,
  "IsNot": false,
  "Children": [ /* MatchConditionBase の配列 */ ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
}
```

**MatchComparison**: `Equal` / `NotEqual` / `LessThan` / `LessThanOrEqual` / `GreaterThan` / `GreaterThanOrEqual` / `Like` / `In` / `NotIn` / `Exists` / `NotExists`

## DBカラム型とフィールド型の対応

| DBカラム型 | フィールド型 |
|---|---|
| UUID / INTEGER (PK) | IdFieldDesign |
| VARCHAR / TEXT | TextFieldDesign |
| INTEGER / DECIMAL / NUMERIC | NumberFieldDesign |
| BOOLEAN | BooleanFieldDesign |
| DATE | DateFieldDesign |
| TIMESTAMP | DateTimeFieldDesign |
| TIME | TimeFieldDesign |
| INTEGER / VARCHAR (FK) | LinkFieldDesign |
| VARCHAR (選択肢) | SelectFieldDesign |
| ファイル関連列3つ | FileFieldDesign |

## 設定ファイル生成のポイント

1. **モジュール名** = PascalCase（例: `ProductCategory`）
2. **フィールド名** = PascalCase（例: `ProductName`）
3. **DBカラム名** = snake_case（例: `product_name`）
4. **全フィールドに `TypeFullName` が必須** - 型の完全修飾名
5. **レイアウトはフィールド名で参照** - `FieldLayoutDesign.FieldName`
6. **モジュール間参照は `SearchCondition.ModuleName`** で指定
7. **スクリプトイベントは文字列で指定** - 対応する `.mod.cs` ファイルにメソッドを記述
8. **`""` キーがデフォルトレイアウト** - 名前付きレイアウトも定義可能
9. **JSON数値型に注意** - `int`型プロパティに `50.0` のような小数点付き数値を書くとデシリアライズエラーになる。各ドキュメントのプロパティ定義にC#型を明記しているので参照すること
10. **ListLayout の Elements 構造** - `Elements[行][列]` の二重配列。通常は `[[Col1, Col2, Col3]]` のように全列を1つの内側配列に入れる。`[[Col1], [Col2]]` だと列が縦に並ぶ（間違い）
11. **LinkField の値パス** - `LinkFieldName.Value` で外部キー値にアクセス。`LinkFieldName.Id.Value` は誤り
12. **子リストの LimitCount** - 詳細画面の DetailListField/ListField は `LimitCount: null`（全件）。ページ一覧では `50`
13. **スクリプトで this** - `this.Submit()`, `this.ValidateInput()`, `this.IsNewData` 等のモジュールメソッド/プロパティにはthisを付ける
14. **AnchorTagField の Target** - 有効な値は `Url` / `HistoryBack` / `HistoryForward` のみ。モジュール遷移は `Target: "Url"` + `Module` プロパティで実現
15. **IsViewOnly はレイアウト要素のプロパティ** - フィールド定義（Fields配列）ではなく、FieldLayoutDesign / GridLayoutDesign / ListElement に設定する。詳細は [Docs/Layouts.md](Docs/Layouts.md) の IsViewOnly セクション参照
16. **一覧ページのソートは PageFrame で設定** - モジュールの `ListPageFieldDesign` ではなく、PageFrame の `ListPageDesign.ListFieldDesign.SearchCondition.SortConditions` で設定する。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) のソート設定の優先順位を参照
17. **LinkFieldNames** - 他モジュールのフィールドを表示するには `LinkFieldNames` にパスを追加し、レイアウトでも参照が必要。詳細は [Docs/ModuleDesign.md](Docs/ModuleDesign.md) の LinkFieldNames の詳細を参照
18. **IsUpdateProtected と IsViewOnly の違い** - `IsUpdateProtected` はサーバーサイドでも更新防止（フィールド定義）、`IsViewOnly` は表示上の読み取り専用（レイアウト要素）。混同しないこと
19. **表示専用モジュールの ListLayout にフィールドを入れない** - `DbTable` が空のモジュールでは `ListLayouts` の Elements にフィールドを入れるとと一覧表示モジュールと認識されてしまう。Id / SubmitButton も不要。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #22 を参照
20. **よくある間違い** - 詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) を参照
21. **スクリプトで `await` を使わない** - スクリプトエンジンが非同期メソッドを自動で同期処理する。`await this.Submit()` ではなく `this.Submit()` と書くこと。`await MessageBox.Show()` ではなく `MessageBox.Show()` と書くこと
22. **Null条件演算子（`?.`）は使用可能** - `product?.Name.Value` のようにnull安全なプロパティアクセス・メソッド呼び出し・チェーンが可能。`??` と組み合わせて `product?.Name.Value ?? "default"` も使える。ただし **Null条件インデクサ（`?[]`）は使えない**。Null合体演算子（`??`）も使用可能
23. **サイドバーの階層メニュー** - PageFrame の `Title` に `/` 区切りで文字列を指定すると階層メニューが自動生成される（例: `"マスタ/取引先"` → 「マスタ」グループ配下に「取引先」リンク）。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) の Title による階層メニュー を参照
24. **カラム枠線は BorderStyle で設定** - `Border` プロパティ（"None"/"All"等）は非推奨。`BorderStyle` の太さ（`Left`/`Top`/`Right`/`Bottom`）と色（`LeftColor`等）の両方を設定すること。太さを指定しないと線は表示されない。隣接セルとの重複回避ルールあり。詳細は [Docs/BorderStyleGuide.md](Docs/BorderStyleGuide.md) を参照
25. **リストのチェックボックスには BooleanField を使う** - `CanSelect` は行選択ハイライトであり、チェックボックスではない。チェックボックスは `BooleanFieldDesign`（UIType: CheckBox）をListElementに `IsViewOnly: false` で配置し、`CanSelect: false` にする。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #26 を参照
26. **表示専用モジュールでも入力があるなら CanUpdate: true** - `DbTable` が空のモジュールでも、リスト内に入力可能フィールド（チェックボックス、枚数入力等）がある場合は `CanUpdate: true` にしないと画面全体がViewOnlyになる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #27 を参照
27. **FieldValueMatchCondition の Value には TypeFullName が必須** - `Value` プロパティは `MultiTypeValue`（抽象クラス）のため、`StringValue` / `DecimalValue` / `BooleanValue` 等の TypeFullName を必ず指定する。例: `{"Value": "AH", "TypeFullName": "Codeer.LowCode.Blazor.Repository.StringValue"}`。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #28 を参照
28. **外部キーには NumberField を使わない** - 親子関係の外部キーフィールドには `LinkFieldDesign` または `IdFieldDesign` を使う。`NumberFieldDesign` は不可。フレームワーク内部で親レコード未保存時にテンポラリ ID（文字列）が一時的に使われるため。DB カラムの型は実際の ID 型（INTEGER 等）でよい。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #23 を参照
29. **ViewEditToggleButton は SubmitButton を初期非表示にする** - ViewEditToggleButtonField をモジュールに置くと、初期化時に同一モジュール内の全 SubmitButton が `IsVisible = false` になる（編集モードに入ったときだけ表示）。知らないと「Submit が消えた」バグに見える。詳細は [Docs/Fields/ViewEditToggleButtonField.md](Docs/Fields/ViewEditToggleButtonField.md) と [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #29 を参照
30. **Grid 中央配置は空セルパターンを使う** - `[空 \| content \| 空]` のように Layout=null の空セルで挟むと、中身はコンテンツサイズで中央配置、左寄せは `[content \| 空]`、右寄せは `[空 \| content]`。詳細は [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の Grid 中央配置パターンを参照
31. **サイドバー/ヘッダーをモジュール化できる** - `SideBarDesign.ModuleName` / `HeaderDesign.ModuleName` に表示専用モジュールを指定すると、標準UIの代わりにそのモジュールの `DetailLayouts[""]` が描画される。Home/Links/Logout は出なくなるので必要なら自前実装する。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) の ModuleName セクションを参照
32. **ReloadWithLock() は存在しない** - 古いバージョンのスクリプト例に `module.ReloadWithLock()` を呼ぶものが残っていることがあるが、現在の公開APIには存在しない。再読込は `module.Reload()` を使う。ロック付き再読込が必要なら `ExecuteSqlField` で `SELECT ... FOR UPDATE` する方針を検討。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #30 を参照
33. **CanvasElement に Name プロパティはない** - スクリプトから個別 Element を直接操作する手段はない。Element 内に `GridLayoutDesign` 等をネストして、その Layout の `Name` 経由でアクセスする。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #32 を参照
34. **存在しない FieldDesign クラスを使わない** - `CreatedByFieldDesign` / `ChildModuleFieldDesign` / `ImageFieldDesign` 等は CLB に実在しない。実在クラスは `Source/Codeer.LowCode.Blazor/Repository/Design/*FieldDesign.cs` と [Docs/Fields/](Docs/Fields/) で確認。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #34 を参照
35. **一覧ページで編集可能にしない** - PageFrame の Auto/ListToDetail で `ListFieldDesign.CanCreate/CanUpdate` を `true` にすると一覧画面で行内編集 UI が出る。一般的な業務アプリでは一覧で閲覧 + 削除のみ、新規/編集は詳細画面で。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #35 を参照
36. **一覧→詳細フローの詳細画面に戻るボタンを置く** - `AnchorTagFieldDesign` の `Target: "HistoryBack"` + `Icon: "bi bi-arrow-left-circle-fill"` + `FontSize: 30` を DetailLayout の 1 行目に配置。`Samples/PatternShowcase/App/Modules/Product.mod.json` 等の詳細レイアウト参考。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #36 を参照
37. **デザイン系新規作成時は C# プロパティのデフォルト値を踏襲する (重要)** - Module / Field / Layout / PageFrame / PageLink などを生成するときは、推測でプロパティ値を埋めず、`Source/Codeer.LowCode.Blazor/Repository/Design/*.cs` の C# 初期値をそのまま使う。確たる理由がない限り変更しない。例: `ModuleDesign.CanCreate/CanUpdate/CanDelete` は全部 `true` がデフォルト。表示専用モジュールでも `CanUpdate: true` にしないと入力フィールドが ViewOnly になる ([#27](Docs/CommonMistakes.md#27)参照)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #37 を参照
38. **PageFrame に登録されてないモジュールは真っ白になる** - 親→子の詳細遷移先 / ダイアログモジュール / Lookup マスタ等、サイドバーに出さなくても画面遷移する可能性があるモジュールは PageFrame の `OtherPageModuleDesigns` に登録する。`ChildModuleFieldDesign` (= `ModuleFieldDesign`) で埋め込みするだけのモジュールは登録不要。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #38 を参照
39. **SearchLayouts は空辞書ではなく `""` キーの空レイアウトを必ず作る** - `SearchLayouts: {}` のままだと `GetSearchCondition()` が空 `SearchCondition` を返し、`ListField.SetAdditionalConditionAsync` で `モジュール名が一致しません` 例外で真っ白になる。検索画面が要らないモジュールでも `SearchLayouts[""]` に空の `SearchGridLayoutDesign` を入れる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #39 を参照
40. **親子 ListField は `SearchCondition.Condition` で絞り込みを書く** - `LinkFieldNames` は参照宣言だけで、フィルタは別途。`FieldVariableMatchCondition` で `SearchTargetVariable: "OrderId.Value"` (子FK) / `Variable: "Id.Value"` (親PK) を `Equal` で指定。多対多も同パターンで中間テーブル経由。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #40 を参照
41. **論理削除は Field名を `LogicalDelete` にする** - `SystemFieldNames.LogicalDelete` 予約名の Boolean がある場合のみ、削除ボタンが `UPDATE SET LogicalDelete=true` に変わる。任意の `IsDeleted` 等では物理削除のまま。CLB の自動論理削除フィルタも同じ予約名でだけ動く。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #41 を参照
42. **楽観ロックは `IncrementVersion: true` で動かす** - `OptimisticLockingFieldDesign` のデフォルトは `false` (PostgreSQL `xmin` 前提)。SQLite/SQL Server/MySQL でアプリ側のバージョン管理が必要なら `IncrementVersion: true` を明示。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #42 を参照
43. **CSV インポート/エクスポートは PageFrame の Link 側で有効化** - `Link.ListPageDesign.CanBulkDataUpdate/CanBulkDataDownload` を true にする。モジュール側の同名プロパティだけでは一覧画面のアイコンボタンが出ない。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #43 を参照
44. **一覧ページに行選択+一括処理ボタンは置けない** - ListPage は内部で `$List$Module` を動的生成し、システムフィールド (`ListPageList`/`ListPageSearch`/`ListPageSubmit`) のみで構成されるため、任意のボタンを足せない。一括削除/更新は「親 (表示専用) モジュールの詳細レイアウト」に ListField + ボタンで実装し、PageLink を `ModulePageType: "Detail"` にする。子の ListLayout に `IsSelected` (BooleanField, `DbColumn` なし) を出して、スクリプトで `foreach (var row in Items.Rows) if (row.IsSelected.Value == true) Items.DeleteRow(row);` → SubmitButton で DB 反映。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #44 を参照
45. **Id (連番システム ID) は表示しない、ただし DB-backed モジュールは Id 定義必須** (絶対常識) - ListLayout/DetailLayout のどちらにも `IdFieldDesign` の `Id` フィールドを出さない (空セルになる、業務的に無意味)。一方で **`DbTable`/`DataSourceName` を持つモジュールは Fields 配列の Id 定義が必須** — これがないと CLB が一覧→詳細遷移の URL `/Module/{id}` を組まず、`>` ボタンが効かない、`ListField` の親子フィルタ (`Variable: "Id.Value"`) も解決しない。**表示専用モジュール (`DbTable: ""`/`DataSourceName: ""`、ダイアログ・ダッシュボード等) は Id 不要** (どのデータを表示するか特定が要らないため)。行番号が欲しい時は `ListNumberFieldDesign` を別途追加。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #45 を参照
46. **論理削除モジュールでは LogicalDelete を UI に出さない** - `LogicalDelete` (BooleanField 予約名) は Fields に定義するが、ListLayout/DetailLayout のどちらにも入れない。フラグ確認用に管理画面が必要なら**別モジュール**を作って同じ DbTable + Boolean 名を `LogicalDelete` 以外 (例: `DeletedFlag`) にして CLB の自動フィルタを回避。管理モジュールは `CanDelete: false` (物理削除誤防止) + `CanNavigateToDetail: true` (詳細で復活)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #41 を参照
47. **CLB システムフィールドは `SystemFieldNames` 予約名で定義する (絶対常識)** - `Id` / `LogicalDelete` / `OptimisticLocking` / `CreatedAt` / `UpdatedAt` / `Creator` / `Updater` は CLB の予約名。Field の `Name` をこれら**そのままの綴り**にすると、ランタイムが自動で振る舞う (主キー扱い / 削除フィルタ / 楽観ロック / 時刻自動セット / ユーザー自動セット)。`Version`/`IsDeleted` 等の任意名では自動動作が**一切効かない**。`DbColumn` は別途任意の DB 列名でよい。`Id`/`LogicalDelete`/`OptimisticLocking` は **UI 非表示** が原則。**`Creator`/`CreatedAt`/`Updater`/`UpdatedAt` は CLB の保存処理が自動でセットするので、スクリプトで `Creator.Value = CurrentUser.Id.Value` のように代入する必要はない** ([Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #42-A を参照 (`SystemFieldNames.cs` ベースの一覧表)
48. **検索ページのスクリプトは `SearchValue`/`SearchMin`/`SearchMax` を使う (絶対常識)** - `OnSearchInitialization` 等の検索コンテキストで `Status.Value = "進行中"` のように **`.Value` をセットしても無視される**。検索ページのフィールドは検索専用プロパティ系統で動く。単一値系 (Text/Id/Link/Boolean/Select/RadioGroup) は `SearchValue`、範囲系 (Number/Date/DateTime/Time = `RangeSearchField`) は `SearchMin`/`SearchMax`。Text/Id/Link は `SearchComparison` も併用。`OnSearchInitialization` スクリプトは URL に `?initialize_search=true` が付いている時だけ発火 (サイドバー Link 経由は自動付与、直接 URL は付かない)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #46 を参照
49. **DetailListField/TileListField で並べる Module の DetailLayout はカード化する (絶対常識)** - DetailListField や TileListField で参照される行/タイル単位の Module は、`DetailLayouts[""].Layout.IsBordered: true` でカード化必須。ListField (表形式) は表自体が罫線で区切るので不要だが、DetailListField/TileListField はレコードがフォーム/タイルとしてただ並ぶだけなのでカード化しないと境界が見えず、複数レコードが連続したフォームに見える。一覧ページの `ListPageDesign.ListFieldDesign` を `DetailListFieldDesign`/`TileListFieldDesign` に差し替えるパターンでも同じ。行 Module を他用途で使い回す場合は専用の DetailLayout 名 ("Card" 等) を追加して `LayoutName` で切り替える。詳細は [Docs/Fields/DetailListField.md](Docs/Fields/DetailListField.md) のカード化セクションを参照
50. **`FieldBase.LayoutName` と `*FieldDesign.LayoutName` を混同しない** - `FieldBase.LayoutName` は `Parent?.LayoutName ?? ""` で親 Module の LayoutName を継承する値 (Detail Layout 名)。一方、`SearchFieldDesign.LayoutName` / `ListFieldDesign.LayoutName` 等は「Design 側で指定する参照先 Layout 名」(Search/List Layout のキー)。同じ「LayoutName」だが意味が違う。`field.LayoutName` (継承) でなく `field.Design.LayoutName` (Design 経由) を使うのがほぼ常に正解。製品コード側では `ListPageComponentVM.GetSearchLayout()` が前者を使っていて SearchLayoutName 指定が無視されるバグになっていた (修正済)。詳細は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) の落とし穴セクションを参照
51. **一覧ページ・カスタム一覧の組み立ては パターン集を引く** - 「一覧ページ作って」「Excel 入出力」「Detail/Tile 形式で並べたい」「並び順固定」「カスタム一覧 (検索 + サマリ + List)」等の指示は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) に再現可能なレシピがある。SearchLayouts 空辞書禁止、複数 Link を同一 Module に置くときの ModuleUrlSegment 分離、列幅は1列だけ可変にして末尾ボタン列を伸ばさない、SearchField を使うパターンと SearchLayout に Field 参照が必要なことなど、レイアウトずれ・無描画の落とし穴も同ドキュメントに集約。詳細は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) を参照
52. **PageFrame の Link / Module / Field / Layout 等の複雑 JSON を「ゼロから書き起こさない」 (致命的)** - 特に PageFrame `Link` (ListPageDesign / DetailPageDesign / ListFieldDesign など深いネスト) を Python で組み立てて作ると、必須プロパティが 1 つでも欠けるだけで **PageFrame 全体の読み込みが失敗し、サイドバーが完全に消え画面真っ白** という致命的事態になる。スクリプトで Link を生成するときは **必ず既存 Link を `copy.deepcopy()` して、`Title` / `Module` / `ListFieldDesign.LayoutName` 等の必要箇所だけ差し替える** こと。Module / Field / Layout / SearchCondition も同様で、デフォルト値のうち網羅が困難なものがある (`IsStriped` / `IconType` / `HideTitle` 等の見落とし常連) ため、既存ファイルからの deepcopy が安全。新規モジュールを 1 から組むときは、Designer で 1 件雛形を作って commit → それを毎回 deepcopy する運用にする。スキーマを覚えで書くのは禁止。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #47 を参照
53. **FileField / ImageField はサーバ側設定 + 一時ファイルテーブルが必要 (重要)** - `FileField` を JSON に置くだけでは動かず、ランタイムで添付エラーになる。サーバ側 `appsettings.json` の `TemporaryFileTableInfo` (DataSource毎)・対応する `temporary_files` テーブル (DataSource毎)・`designer.settings.json` の `FileStorages` の 3点セットが揃って初めて動作する。サンプル/ショーケースに添付サンプルを入れるときは事前にこれらが用意されているか必ず確認すること (入れたが動かないと「ただのデモ崩れ」になる)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #48 と [Docs/Fields/FileField.md](Docs/Fields/FileField.md) の「サーバ側設定が必須」セクション参照
54. **機能存在の根拠は CLB コアソース、サンプル側 Description は信用しない (致命的)** - サンプル Module の Description テキストや過去の README に「CSV ダウンロードできます」「PDF出力対応」等と書いてあっても、それは過去の AI 生成や見切り発車で書かれた**フィクションの可能性**がある。機能の実在は必ず `Source/Codeer.LowCode.Blazor/` 配下のコアソースを grep して裏取りすること。例: `CanBulkDataDownload` は `ListPageComponent.razor` で `DownloadExcel()` を呼ぶ Excel 専用機能で、CSV 機能は CLB に存在しない。ホーム説明・ドキュメント・新規サンプルを書くときに「サンプル側のテキストを根拠」にしてはいけない。**裏取りなしで機能名を書くのは禁止**。サイドバー Link 表記とも整合させる (「CSVダウンロード」「CSVエクスポート」のような同義語でも乖離扱い)。サンプル Link の追加/削除/リネーム時は `Home.mod.json` / `home.txt` / 関連ドキュメントも同タイミングで同期する
55. **`GridRow.KeepInFillAvailableGrid` は Claude が `true` にしない。ユーザ操作専用 (絶対)** - `IsFillAvailable=true` の Grid の最終行 (FillAvailable target) には2モードあり、デフォルト `false` = DirectList モード (`ListField` / `ProCodeField` の内部スクロールが画面下端まで広がる、これがほぼ全ケースの正解)、`true` = FitContent モード (`Button` / `Label` のような固定高さ最終行用、超絶レア)。**ユーザから「最終行フィットモード」「KeepInFillAvailableGrid を true に」と明示的な指示があった場合でも、Claude は `true` にしない**。立てる判断は Designer 上でユーザが手動でやる前提。ユーザの言葉に "true" や "FitContent" が出ても、Claude 側は **常に `false` のまま**。立てたい時はユーザが Designer GUI でチェックを入れる。`ListField` を最終行に置くシナリオで `true` を立てると **FillAvailable の効果が消えて画面下端まで広がらなくなる**致命的な見た目バグになる。プロパティ名に "FillAvailable" が入っているせいで「FillAvailable させる ON フラグ」と誤解しがちだが、意味は逆 (`false` が普通の FillAvailable 動作、`true` は特殊モード切替)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #49、`GridRow` プロパティ表は [Docs/Layouts.md](Docs/Layouts.md) 参照
56. **「ラベル列 + 入力列」 2カラムフォームでは、ラベル列に `VerticalAlignment: "Middle"` を必ず設定 (マスト)** - `<ラベル(80-120px)> | <入力欄(伸縮)>` の典型レイアウトでは、ラベル列の Column に **必ず `VerticalAlignment: "Middle"` を設定する**。これが無いと、入力列が縦に伸びるケース (Multiline TextField, FileField のプレビュー枠, 長文プレースホルダで折り返し, ListField/DetailListField を埋め込んだ複合フォーム等) で **ラベルだけがセル上端に張り付き、入力欄の中央とずれて見栄えが崩れる**。短文の単行入力時には差が出ないので見落としやすいが、後で必ず破綻するので**最初から全てのラベル列に Middle を入れておく**のが正解。地味だが業務UI品質には致命的に効く。既存サンプル (`FormLayoutSample` / `EditDialogTarget` / `ShowPanelTarget` 等) は全てこれが入っている — そのまま deepcopy すれば自然に守られる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #50 を参照
57. **スクリプトに名前付き引数 (`name: value`) は書けない** - CLB のスクリプトインタプリタは C# の名前付き引数構文を非サポート。`new PrimaryButton("確定", isDefault: true)` のように書くとロード時に「`isDefault: 不正なシンタックスです`」エラー。必ず位置引数で `new PrimaryButton("確定", true)`。`MessageBox.ShowWithTitle(title: ...)` 等も同様。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照
58. **`Module.Submit()` を呼んだら必ず戻り値を確認する + ローディングは `LoadingService.StartLoading(delay)` で遅延表示** - `Submit()` の戻り値は `bool?` で `null`/`false`/`true` の3パターン (`null`=送信データなし、`false`=通信/検証失敗、`true`=成功)。失敗時は `Toaster.Error(...)` で通知すべきで、戻り値を捨てて `Items.Reload()` だけ走らせるのは NG。また `Module.Submit()` / `Module.Reload()` は HTTP 通信中に CLB 標準のローディングオーバーレイを瞬時に出してチラつくので、`using var scope = LoadingService.StartLoading(1000);` のスコープを張って「1秒以上かかる場合のみ表示」にすると UX が良い。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) の「Submit() / Reload() を呼んだ後は結果を確認する」セクション参照
59. **`SearchTargetVariable` / `Variable` 系プロパティは Variable (`Id.Value`) であって Field (`Id`) ではない (致命的)** - SearchCondition / FieldValueMatchCondition / FieldVariableMatchCondition / LinkField.ValueVariable / SortCondition.Variable 等、プロパティ名に **Variable** が入っているところには `Field名 + "." + データメンバ名` (例: `Id.Value`, `Status.Value`, `Category.DisplayText`) を書く。Field 名だけ (`Id` / `Status`) を書くと `Id フィールドが存在しません` 等の例外で SQL 生成が失敗する。逆に `SearchCondition.SelectFields` のように プロパティ名に **Field** が入っているところは Field 名のみ。デザイナでもこの違いを混同しやすいので注意。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #51 を参照
60. **ChildModule の LinkField/SelectField は `.Value` がロード遅延で空のことがある (致命的)** - `DetailListField` 経由の孫 Module (例: `Order.Members[i]`) は親モジュールの `OnAfterInitialization` タイミングで LinkField/SelectField の `.Value` が `""` (空) のことがある。`if (m.Status.Value != "Waiting")` のような判定が常に true → continue で全件スキップされる罠。対処は `ModuleSearcher` で DB から直接検索 (`AddEquals(m => m.ApprovalFlowOrderId.Value, o.Id.Value)` のように Order.Id を使って絞り込む。`o.Id.Value` は ChildModule でも取れる)。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) の「ChildModule の LinkField/SelectField」セクション参照
61. **動的型は `var` で受ける (致命的)** - `GetParentModule()` の戻り値や `Rows[i]` のメンバは動的型 (`object`)。`bool isNew = parent.IsNewData;` のような明示型代入は「**bool = var 不正な代入です**」ロード時エラーになる。`var isNew = parent.IsNewData;` で受ける
62. **式メソッド `=>` と複数引数ラムダ `(a,b)=>...` は使えない** - `string F() => "x";` (expression-bodied method) は不可、ブロック体 `string F() { return "x"; }` で書く。`list.Sort((a,b) => ...)` も不可、手動ソートする。単一引数ラムダ `x => x.Value` は `ModuleSearcher` の条件メソッド内のみ可。詳細は [Docs/Scripts.md](Docs/Scripts.md) のラムダ式セクション + [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照
63. **レイアウトは「削れるだけ削る」(デザイン基本原則)** - 囲みラベル / 入力欄ラベル (placeholder で代用) / DetailListField の DisplayName / ページャー / ユーザーソート / 単独「状態」「担当者」表示 / 連番 Id 列 / 状態に応じないボタン などは削れる候補。複数フィールドを 1 行のテキストに統合する発想も有効 (例: `状態:進行中  ✓A→▶B→C` のように Status + フロー + 現在担当を 1 行のマーカー付き文字列で表現)。削減チェックリストと統合パターンは [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の「レイアウト最小化の原則」セクション参照
64. **複数フィールド更新 + 通信は `SuspendNotifyStateChanged` + `LoadingService.StartLoading` でまとめる** - ボタンハンドラ (承認/却下/キャンセル等) で `Field.Value = ...` を連続させると毎回 StateHasChanged が走って画面チラつく。`using var s = GetParentModule().SuspendNotifyStateChanged();` で再描画を 1 回にまとめる。さらに `LoadingService.StartLoading(int? delay)` は (1) `delay=0` で**複数通信のローディングを 1 個にまとめる** (2) `delay=1000` で**短時間処理ならインジケータを出さない** の 2 通り。組合せ例: `using var suspend = GetParentModule().SuspendNotifyStateChanged(); using var loading = LoadingService.StartLoading(0);` 詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) と [Docs/Scripts.md](Docs/Scripts.md) 参照
65. **検索条件の Select→Select 連動は `OnSearchDataChanged` で `SearchValue` → `Value` をコピーする** - `SelectField.SearchCondition` + `FieldVariableMatchCondition` の宣言的連動 (CascadeInputBySearch パターン) は、検索レイアウトに置くと**そのままでは連動しない**。候補絞り込みの `Variable` が参照するのは `Value` だが、検索フォームの入力は `SearchValue` に入るため。親 Select の `OnSearchDataChanged` に `親.Value = 親.SearchValue;` の 1 行スクリプトを足すと、連動先の候補がリアルタイムに取り直され、候補から外れた子の選択値は自動クリアされる。実装サンプル: `Samples/PatternShowcase/App/Modules/CascadeSearch.mod.json` (サイドバー「検索/Select連動」)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #52 を参照
66. **明細の総入れ替え・一意キーの入れ替えは `ListField` の洗い替え (`ReplaceMode`) を使う** - `All` = Submit 時に `SearchCondition` 一致分を全件削除して現在行を全件新規追加 (完全洗い替え。条件が空だとサーバー側ガードで拒否されるため親子構成で使う)、`UpdateAsDeleteInsert` = 変更行だけ削除+新規追加に置き換え (座席番号・表示順など UNIQUE 制約列の値入れ替えが UPDATE の制約違反にならない)。どちらも作り直された行の Id は振り直される。実装サンプル: `ReplaceAllSample` / `SeatReplaceSample` (サイドバー「データ操作/洗い替え (完全)/(更新行)」)。詳細は [Docs/Fields/ListField.md](Docs/Fields/ListField.md) の洗い替えセクションと [Docs/AppPatterns/replace_mode.md](Docs/AppPatterns/replace_mode.md) を参照
67. **候補が少ないマスタ参照は LinkField でなく SelectField (マスタ参照)** - 費目・区分・ステータスのように候補が数件〜十数件でドロップダウンから直接選べるマスタは、検索ピッカーの `LinkFieldDesign` ではなく `SelectFieldDesign`（マスタ参照: `Candidates: []` + `SearchCondition.ModuleName` + `ValueVariable`/`DisplayTextVariable` + `DbColumn`）を使う。「マスタで管理＝LinkField」ではない。LinkField は候補が多く自由検索で 1 件絞り込むとき。正典: `Samples/PatternShowcase/App/Modules/CascadeSearch.mod.json` の `SelectedProject`。詳細は [Docs/Fields/SelectField.md](Docs/Fields/SelectField.md) / [Docs/Fields/LinkField.md](Docs/Fields/LinkField.md) の「使い分け」参照
68. **一覧専用モジュールは `DetailLayouts: {}` にする** - 親の `ListField`（表形式）にインライン表示するだけで詳細ページへ遷移しない（`CanNavigateToDetail: false`）子明細モジュールは `DetailLayouts` を空 `{}` にして詳細デザインを作らない。`ListField` の行は `ListLayouts[""]` で描画され `DetailLayouts` は未使用なので、作り込むと使われないラベル・カードが増えて冗長。逆に詳細遷移する / `DetailListField`・`TileListField` でフォーム・タイル表示するモジュールは `DetailLayouts[""]` が必要（後者はカード化 `IsBordered: true` も必須）。詳細は [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の「一覧専用モジュールは DetailLayouts を作らない」参照
69. **承認・ワークフローは求められない限り作らない／非認証アプリに承認を足さない** - 既定は CRUD（ヘッダ＋明細＋合計等）に留め、承認ボタン等を勝手に足さない・「推奨」もしない。承認は本来 承認者・段階・差し戻し・履歴・権限を伴う重い機能で、認証（ログイン）が前提。非認証アプリでは申請者と承認者を区別できず無意味。承認が必要なら認証前提で `Samples/PatternShowcaseAuth/` の承認フロー（`ApprovalFlow` 系）として正式に作る
70. **明細表 (ヘッダ＋明細) は `ListField`。`DetailListField` ではない (致命的・最頻出の誤り)** - 「注文＋明細」「請求書＋明細」「経費精算＋明細」のように日付・科目・金額… を**列の揃った表**で 1 行ずつ並べる明細は **`ListFieldDesign`** を使い、**列定義は子 (行) モジュール側の `ListLayouts[""].Elements`** に書く。子の親 FK は `IdFieldDesign` (`IsManualInput:false`)。`DetailListFieldDesign` は名前に「明細 (Detail)」が入っているので「明細表＝DetailListField」と短絡しがちだが**意味は逆** — "Detail" は「各行を DetailLayout (フォーム/カード) で描く」の意味で、業務の「明細行」ではない。`DetailListField`/`TileListField` を選ぶのは「1 レコード＝1 枚のフォーム/カード」にしたいと明確に判断したときだけ (その場合は子を `IsBordered:true` でカード化必須)。**迷ったら `ListField`。** 実装に迷ったら正典 `Samples/PatternShowcase/App/Modules/Order.mod.json` (`Details`＝`ListFieldDesign`) と `OrderDetail.mod.json` (`OrderId`＝`IdFieldDesign`、列は `ListLayouts`) を必ず開いて型を確認する。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #53 / [Docs/AppPatterns/header_detail.md](Docs/AppPatterns/header_detail.md) を参照
71. **PageFrame は最低 1 つ `IsApplicationRoot: true` (ルート URL の着地フレーム) を作る (絶対常識)** - `IsApplicationRoot` の C# 既定は `false`。新規 PageFrame を起こして着地フレーム (通常 `Main`) に `true` を立て忘れると、プロジェクトに application root が 0 件になり、ルート URL (`/`) を開いたとき CLB が**非 application-root のフレームにフォールバック**する。`UserReadCondition` で権限ゲートした管理フレーム (`AdminFrame` 等) が既定ランディングに選ばれる事故になる (admin だけ管理画面がホームになる等)。**着地フレームは `true`、補助フレームは `false`**。複数の `true` は PC/スマホ出し分け (`TargetDevice`/`WidthFrom`/`Priority`) のときだけ。単一フレームのプロジェクトでもその 1 枚を必ず `true` にする (空テンプレ複製時に踏みやすい)。正典: `Samples/PatternShowcaseAuth/App/PageFrames/Main.frm.json` (`true`) + `AdminFrame.frm.json` (`false`)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #54 / [Docs/PageFrame.md](Docs/PageFrame.md) 冒頭ルールを参照
