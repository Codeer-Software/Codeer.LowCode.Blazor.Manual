# ページフレーム定義 (*.frm.json)

ページフレームはアプリケーションのナビゲーション構造を定義する。サイドバー、ヘッダー、ページ遷移先のリンクを構成し、アプリケーション全体の骨格となる。

## ファイル命名規則

```
{FrameName}.frm.json
```

- PageFrames フォルダ内に配置
- フレーム名は PascalCase（例: `Main`, `AdminFrame`）

---

## PageFrameDesign プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `IsApplicationRoot` | bool | `false` | アプリケーションのルートフレームかどうか |
| `Name` | string | `""` | フレーム識別名 |
| `Description` | string | `""` | 説明文 |
| `Left` | SideBarDesign | | 左サイドバー |
| `Right` | SideBarDesign | | 右サイドバー |
| `Header` | HeaderDesign | | ヘッダー |
| `Padding` | ThicknessDesign | `{}` | コンテンツ領域の余白 `{ Left, Top, Right, Bottom }` |
| `UserReadCondition` | ModuleMatchCondition | | フレームへのアクセス権限条件 |
| `BackgroundColor` | string | `""` | 背景色 |
| `Color` | string | `""` | 文字色 |
| `FontFamily` | string | `""` | フォントファミリー |
| `FontSize` | int? | null | フォントサイズ。**整数のみ（`14.0` は不可、`14` と書くこと）** |
| `TopPageModuleDesign` | ModulePageDesign | | デフォルト/ホームページの設定 |
| `OtherPageModuleDesigns` | List\<ModulePageDesign\> | `[]` | サイドバー以外の追加ページ定義 |

---

## SideBarDesign - サイドバー定義

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `IsVisible` | bool | `false` | サイドバーを表示するか |
| `Home` | HomeLabel | | ホームリンクの表示設定 |
| `Links` | List\<PageLink\> | `[]` | ナビゲーションリンクのリスト |
| `Width` | double? | null | サイドバーの幅（px） |
| `Color` | string | `""` | 文字色 |
| `BackgroundColorStart` | string | `""` | グラデーション開始色 |
| `BackgroundColorEnd` | string | `""` | グラデーション終了色 |
| `FontFamily` | string | `""` | フォントファミリー |
| `FontSize` | int? | null | フォントサイズ。**整数のみ（`14.0` は不可、`14` と書くこと）** |
| `UserName` | string | `""` | ユーザー名表示用フィールド |
| `ModuleName` | string | `""` | サイドバー用モジュール名（標準UIの代替、下記参照） |
| `MobileBehavior` | MobileBehavior | `"CollapseToHamburger"` | モバイル（狭幅）時の挙動。`None` / `CollapseToHamburger` |

### MobileBehavior

| 値 | 説明 |
|---|---|
| `None` | 狭幅時もサイドバーを常時表示したまま |
| `CollapseToHamburger` | 狭幅時にハンバーガーメニューに折りたたむ（デフォルト） |

### ModuleName によるサイドバーのモジュール化

`SideBarDesign.ModuleName` に表示専用モジュール名を指定すると、標準サイドバー UI（Home / Links / UserName / Logout）の代わりに、そのモジュールの `DetailLayouts[""]` が `ModuleRenderer` 経由で描画される。

**用途:** サイドバーの見た目や構造を完全にカスタマイズしたい場合（画像、独自のナビゲーション、動的メニューなど）。

**使用時の制約:**

- モジュールは **表示専用モジュール**（`DbTable: ""`, `DataSourceName: ""`, `IdFieldDesign` なし, `SubmitButtonFieldDesign` なし, `ListLayouts.Elements` 空）にする
- 描画されるのは `DetailLayouts[""]` のみ（他の名前付きレイアウトは使われない）
- 標準サイドバーの Home / Links / Logout は **出ない**。必要なら `AnchorTagFieldDesign` で自前実装する
- サイドバー幅（`Width`）の制約内で収まるようにレイアウトを作ること
- `IsVisible: true` にすることを忘れない

**JSON例:**

```json
"Left": {
  "IsVisible": true,
  "ModuleName": "SidebarModule",
  "Width": 280,
  "BackgroundColorStart": "#1A237E",
  "BackgroundColorEnd": "#311B92",
  "Home": { "Type": "Text", "Text": "" },
  "Links": []
}
```

対応する表示専用モジュール `SidebarModule.mod.json` の例:

```json
{
  "Name": "SidebarModule",
  "DataSourceName": "",
  "DbTable": "",
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "Fields": [
    {
      "Text": "ホーム",
      "Target": "Url",
      "Module": "Home",
      "Name": "HomeLink",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
    }
  ],
  "DetailLayouts": {
    "": {
      "Layout": {
        "Rows": [
          { "Columns": [{ "Layout": { "FieldName": "HomeLink", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } }] }
        ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
      }
    }
  }
}
```

ヘッダー (`HeaderDesign.ModuleName`) も同じ仕組みで動作する。

---

## HeaderDesign - ヘッダー定義

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `IsVisible` | bool | `false` | ヘッダーを表示するか |
| `Home` | HomeLabel | | ホームリンクの表示設定 |
| `Links` | List\<PageLink\> | `[]` | ヘッダーのナビゲーションリンク |
| `Height` | double? | null | ヘッダーの高さ（px） |
| `Color` | string | `""` | 文字色 |
| `BackgroundColor` | string | `""` | 背景色 |
| `FontFamily` | string | `""` | フォントファミリー |
| `FontSize` | int? | null | フォントサイズ。**整数のみ（`14.0` は不可、`14` と書くこと）** |
| `UserName` | string | `""` | ユーザー名表示用フィールド |
| `ModuleName` | string | `""` | ヘッダー用モジュール名 |

---

## HomeLabel - ホームラベル

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Type` | HomeLabelType | `"Text"` | ラベル種別: `Text` / `Image` |
| `Text` | string | `""` | テキスト表示時の文字列 |
| `Icon` | string | `""` | アイコンCSS クラス名（例: `"bi bi-house"`） |
| `ResourcePath` | string | `""` | 画像表示時のリソースパス |

---

## PageLink - ナビゲーションリンク

PageLink は ModulePageDesign を継承し、ナビゲーション表示用のプロパティを追加する。

### PageLink 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Title` | string | `""` | リンクの表示テキスト。`/` 区切りで階層メニューを作成可能（下記参照） |
| `Icon` | string | `""` | アイコン（CSSクラス名またはリソースパス） |
| `IconType` | string | `"Font"` | アイコン種別: `Font` / `ResourceImage` |
| `HideTitle` | bool | `false` | タイトルを非表示にするか（アイコンのみ表示） |

#### Title による階層メニュー

`Title` に `/`（スラッシュ）区切りの文字列を指定すると、サイドバーに階層メニューが自動生成される。

```json
{ "Title": "マスタ/取引先", "Module": "Partner" },
{ "Title": "マスタ/拠点・倉庫", "Module": "Warehouse" },
{ "Title": "マスタ/商品", "Module": "Product" },
{ "Title": "マスタ/ユーザー", "Module": "User" },
{ "Title": "マスタ/自社情報", "Module": "CompanyInfo" }
```

上記の場合、サイドバーには以下のように表示される：

```
├─ マスタ          ← 自動的にグループ見出しになる
│   ├─ 取引先
│   ├─ 拠点・倉庫
│   ├─ 商品
│   ├─ ユーザー
│   └─ 自社情報
```

- `/` の左側がグループ名、右側がリンク名
- 同じグループ名のリンクは自動的にまとめられる
- 複数階層（`A/B/C`）も可能

### ModulePageDesign プロパティ（PageLink が継承）

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ModulePageType` | ModulePageType | `"Auto"` | ページ種別: `Auto` / `ListToDetail` / `List` / `Detail` |
| `ModuleUrlSegment` | string | `""` | URLのセグメント（カスタムURL用） |
| `ActiveModuleSegments` | List\<string\> | `[]` | アクティブとみなすモジュールセグメントのリスト |
| `PageFrame` | string | `""` | 遷移先のページフレーム名（サブフレーム） |
| `Module` | string | `""` | 遷移先のモジュール名 |
| `Id` | string | `""` | 特定レコードのID（Detail時） |
| `Parameters` | string | `""` | URLパラメータ |
| `ListPageDesign` | ListPageDesign | | 一覧ページの設定 |
| `DetailPageDesign` | DetailPageDesign | | 詳細ページの設定 |

---

## ListPageDesign - 一覧ページ設定

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SearchLayoutName` | string | `""` | 使用する SearchLayout 名 |
| `UserUrlParameter` | bool | `true` | 検索条件をURLパラメータに保持するか |
| `PageTitle` | string | `""` | ページタイトル |
| `HeaderTitle` | string | `""` | ヘッダータイトル |
| `CanBulkDataUpdate` | bool | `false` | 一括データ更新を許可 |
| `CanBulkDataDownload` | bool | `false` | 一括データダウンロードを許可 |
| `UseSubmitButton` | bool | `false` | 一覧画面で送信ボタンを使用 |
| `UseNavigateToCreate` | bool | `true` | 新規作成への遷移を許可 |
| `ListFieldDesign` | ListFieldDesign | | 一覧表示の ListField 設定 |

---

## DetailPageDesign - 詳細ページ設定

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `PageTitle` | string | `""` | ページタイトル |
| `LayoutName` | string | `""` | 使用する DetailLayout 名 |
| `UrlParameter` | string | `""` | URLパラメータ |

---

## 完全なJSON例

```json
{
  "IsApplicationRoot": true,
  "Name": "Main",
  "Description": "メインフレーム",
  "Left": {
    "IsVisible": true,
    "Home": {
      "Type": "Text",
      "Text": "Home",
      "Icon": "",
      "ResourcePath": ""
    },
    "Links": [
      {
        "Title": "商品管理",
        "Icon": "bi bi-box",
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
          "PageTitle": "商品一覧",
          "HeaderTitle": "商品管理",
          "CanBulkDataUpdate": false,
          "CanBulkDataDownload": true,
          "UseSubmitButton": false,
          "UseNavigateToCreate": true,
          "ListFieldDesign": {
            "LayoutName": "",
            "CanNavigateToDetail": true,
            "NavigateModuleUrlSegment": "",
            "DisplayName": "",
            "SearchCondition": {
              "LimitCount": 50,
              "SelectFields": [],
              "SortConditions": [],
              "ModuleName": ""
            },
            "PagerPosition": "Bottom",
            "CanDelete": true,
            "CanUserSort": true,
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
          }
        },
        "DetailPageDesign": {
          "PageTitle": "商品詳細",
          "LayoutName": "",
          "UrlParameter": ""
        }
      },
      {
        "Title": "顧客管理",
        "Icon": "bi bi-people",
        "IconType": "Font",
        "HideTitle": false,
        "ModulePageType": "ListToDetail",
        "ModuleUrlSegment": "",
        "ActiveModuleSegments": [],
        "PageFrame": "",
        "Module": "Client",
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
            "SearchCondition": {
              "LimitCount": 50,
              "ModuleName": ""
            },
            "PagerPosition": "Bottom",
            "CanDelete": true,
            "CanUserSort": true,
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
          }
        },
        "DetailPageDesign": {
          "PageTitle": "",
          "LayoutName": "",
          "UrlParameter": ""
        }
      },
      {
        "Title": "ダッシュボード",
        "Icon": "bi bi-graph-up",
        "IconType": "Font",
        "HideTitle": false,
        "ModulePageType": "Detail",
        "PageFrame": "",
        "Module": "Dashboard",
        "Id": "",
        "Parameters": "",
        "ListPageDesign": {
          "SearchLayoutName": "",
          "UserUrlParameter": true,
          "ListFieldDesign": {
            "SearchCondition": { "LimitCount": 50, "ModuleName": "" },
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
          }
        },
        "DetailPageDesign": {
          "PageTitle": "",
          "LayoutName": "",
          "UrlParameter": ""
        }
      }
    ],
    "FontFamily": "",
    "Color": "#ffffff",
    "BackgroundColorStart": "#052767",
    "BackgroundColorEnd": "#3a0647",
    "UserName": "",
    "ModuleName": ""
  },
  "Right": {
    "IsVisible": false,
    "Home": {
      "Type": "None",
      "Text": "",
      "Icon": "",
      "ResourcePath": ""
    },
    "Links": [],
    "Color": "#ffffff",
    "BackgroundColorStart": "#052767",
    "BackgroundColorEnd": "#3a0647",
    "UserName": "",
    "ModuleName": ""
  },
  "Header": {
    "IsVisible": true,
    "Home": {
      "Type": "Text",
      "Text": "My App",
      "Icon": "",
      "ResourcePath": ""
    },
    "Links": [],
    "FontFamily": "",
    "Color": "#ffffff",
    "BackgroundColor": "#1e85eb",
    "UserName": "",
    "ModuleName": ""
  },
  "Padding": {},
  "UserReadCondition": {
    "ModuleName": ""
  },
  "BackgroundColor": "",
  "Color": "",
  "FontFamily": "",
  "TopPageModuleDesign": {
    "ModulePageType": "Detail",
    "ModuleUrlSegment": "",
    "ActiveModuleSegments": [],
    "PageFrame": "",
    "Module": "Home",
    "Id": "",
    "Parameters": "",
    "ListPageDesign": {
      "SearchLayoutName": "",
      "UserUrlParameter": true,
      "ListFieldDesign": {
        "SearchCondition": { "LimitCount": 50, "ModuleName": "" },
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
      }
    },
    "DetailPageDesign": {
      "PageTitle": "",
      "LayoutName": "",
      "UrlParameter": ""
    }
  },
  "OtherPageModuleDesigns": []
}
```

## ソート設定の優先順位

一覧ページのソートには2つの設定場所がある。**PageFrame 側の設定が優先される。**

### 設定場所と優先順位

| 優先順位 | 設定場所 | ファイル | パス |
|---|---|---|---|
| **1（高）** | PageFrame のリンク内 | `*.frm.json` | `Links[].ListPageDesign.ListFieldDesign.SearchCondition.SortConditions` |
| 2（低） | モジュール定義 | `*.mod.json` | `ListPageFieldDesign.SearchCondition.SortConditions` |

### 正しい設定方法

一覧ページのソートは **PageFrame 側で設定する**。

```json
// Main.frm.json の Links 内
{
  "Title": "商品一覧",
  "Module": "Product",
  "ListPageDesign": {
    "ListFieldDesign": {
      "SearchCondition": {
        "LimitCount": 50,
        "SortConditions": [
          { "Variable": "CreatedDate.Value", "IsDescending": true },
          { "Variable": "Name.Value", "IsDescending": false }
        ],
        "ModuleName": ""
      },
      "Name": "",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
    }
  }
}
```

### 例外: 子リスト（DetailListField / ListField）のソート

モジュール内の子リストフィールド（DetailListField, ListField）のソートは、PageFrame ではなく**フィールド自身の SearchCondition** で設定する。これはモジュール定義側で正しい。

```json
// mod.json 内の DetailListField / ListField
{
  "SearchCondition": {
    "LimitCount": null,
    "SortConditions": [
      { "Variable": "SortOrder.Value", "IsDescending": false }
    ],
    "ModuleName": "OrderDetail"
  },
  "Name": "Details",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
}
```

詳細は [CommonMistakes.md](CommonMistakes.md) の #18 を参照。

---

## ModulePageType の選択指針

| 値 | 用途 | 説明 |
|---|---|---|
| `Auto` | 一般的な場合 | モジュール定義のフィールド構成から自動判定 |
| `ListToDetail` | 一覧 → 詳細の遷移型 | 一覧で行選択 → 詳細画面に遷移 |
| `List` | 一覧のみ表示 | 詳細画面への遷移なし |
| `Detail` | 詳細のみ表示 | ダッシュボード、ホーム画面等 |

## サブフレームの利用

PageLink の `PageFrame` プロパティに別のフレーム名を指定することで、サブフレーム（ネストされたナビゲーション構造）を実現できる。

```json
{
  "Title": "管理画面",
  "Icon": "",
  "PageFrame": "AdminFrame",
  "Module": "AdminHome"
}
```
