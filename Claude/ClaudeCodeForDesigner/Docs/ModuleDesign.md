# モジュール定義 (*.mod.json)

モジュールはアプリケーションの画面・データ定義の中核となるファイルである。1つのモジュールが1つの画面（ページ/フォーム）に対応し、フィールド定義、レイアウト、CRUD権限、検索条件等を含む。

## ファイル命名規則

```
{ModuleName}.mod.json
```

- モジュール名は PascalCase（例: `ProductCategory`, `OrderDetail`）
- Modules フォルダ内に配置。サブフォルダによる整理も可能。

## 関連ファイル

| ファイル | 説明 |
|---|---|
| `{ModuleName}.mod.cs` | スクリプト（C#イベントハンドラ） |
| `{ModuleName}.{QueryFieldName}.sql` | QueryField 用の SELECT SQL |
| `{ModuleName}.{FieldName}.sql` | ExecuteSqlField 用の SQL |

## 完全なプロパティ構造

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | `""` | モジュール識別名。PascalCase。アプリ内で一意。 |
| `DataSourceName` | string | `""` | `designer.settings.json` で定義したデータソース名への参照 |
| `DbTable` | string | `""` | マッピング先のDBテーブル名。snake_case推奨。空の場合はDB連携なし。 |
| `CanCreate` | bool | `true` | 新規レコードの作成を許可するか |
| `CanUpdate` | bool | `true` | 既存レコードの更新を許可するか |
| `CanDelete` | bool | `true` | レコードの削除を許可するか |
| `Fields` | List\<FieldDesignBase\> | `[]` | フィールド定義の配列。各フィールドの詳細は Fields/ ドキュメントを参照。 |
| `UserWriteCondition` | ModuleMatchCondition | | ユーザーレベルの書き込み権限条件 |
| `UserReadCondition` | ModuleMatchCondition | | ユーザーレベルの読み取り権限条件 |
| `DataWriteCondition` | ModuleMatchCondition | | 行レベルの書き込みフィルタ条件 |
| `DataReadCondition` | ModuleMatchCondition | | 行レベルの読み取りフィルタ条件 |
| `DetailLayouts` | Dictionary\<string, DetailLayoutDesign\> | `{}` | 詳細画面レイアウト。キー `""` がデフォルト。 |
| `ListLayouts` | Dictionary\<string, ListLayoutDesign\> | `{}` | 一覧画面レイアウト。キー `""` がデフォルト。 |
| `SearchLayouts` | Dictionary\<string, SearchLayoutDesign\> | `{}` | 検索画面レイアウト。キー `""` がデフォルト。 |
| `LinkFieldNames` | List\<string\> | `[]` | このモジュール内の LinkField が参照する先のフィールドパス（例: `"UserCode.Name"`） |
| `ListPageFieldDesign` | ListFieldDesign | | ページ一覧表示時の ListField 設定 |

## ModuleMatchCondition 構造

ユーザー/データレベルのアクセス制御に使用する。

```json
{
  "ModuleName": "AppUser",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [
      {
        "SearchTargetVariable": "Role.Value",
        "Comparison": "Equal",
        "Value": { "Value": "Admin" },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
      }
    ],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

- `ModuleName` が空文字の場合、条件なし（全許可）
- `ModuleName` にユーザーモジュール名を指定し、`Condition` でユーザーの属性をチェックする

## 命名規則

| 対象 | 規則 | 例 |
|---|---|---|
| モジュール名 (Name) | PascalCase | `ProductCategory` |
| フィールド名 (Field.Name) | PascalCase | `ProductName` |
| DBテーブル名 (DbTable) | snake_case | `product_category` |
| DBカラム名 (Field.DbColumn) | snake_case | `product_name` |

## 完全なJSON例

以下は基本的なCRUDモジュールの例。商品情報の登録・編集・削除が可能なモジュールである。

```json
{
  "Name": "Product",
  "DataSourceName": "Data",
  "DbTable": "product",
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "Fields": [
    {
      "DbColumn": "id",
      "IsManualInput": false,
      "Name": "Id",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
    },
    {
      "DbColumn": "name",
      "IsMultiline": false,
      "Placeholder": "商品名を入力",
      "DisplayName": "商品名",
      "IsRequired": true,
      "Name": "Name",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
    },
    {
      "DbColumn": "price",
      "Placeholder": "",
      "Format": "#,##0",
      "Min": 0,
      "DisplayName": "価格",
      "IsRequired": true,
      "Name": "Price",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
    },
    {
      "DbColumn": "is_active",
      "Text": "有効",
      "UIType": "CheckBox",
      "DisplayName": "有効フラグ",
      "Name": "IsActive",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
    },
    {
      "DbColumn": "created_date",
      "Format": "yyyy/MM/dd",
      "DisplayName": "登録日",
      "Name": "CreatedDate",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
    },
    {
      "Text": "登録",
      "Icon": "",
      "Variant": "Primary",
      "IsBlock": true,
      "Name": "SubmitButton",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign"
    }
  ],
  "UserWriteCondition": {
    "ModuleName": ""
  },
  "UserReadCondition": {
    "ModuleName": ""
  },
  "DataWriteCondition": {
    "ModuleName": ""
  },
  "DataReadCondition": {
    "ModuleName": ""
  },
  "DetailLayouts": {
    "": {
      "OnBeforeInitialization": "",
      "OnAfterInitialization": "",
      "OnLocationChanging": "",
      "OnFieldDataChanged": "",
      "DataOnlyFields": [],
      "Layout": {
        "Rows": [
          {
            "IsWrap": false,
            "GridRowType": "Normal",
            "Columns": [
              {
                "Layout": {
                  "FieldName": "Name",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              }
            ]
          },
          {
            "IsWrap": false,
            "GridRowType": "Normal",
            "Columns": [
              {
                "Layout": {
                  "FieldName": "Price",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              },
              {
                "Layout": {
                  "FieldName": "IsActive",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              }
            ]
          },
          {
            "IsWrap": false,
            "GridRowType": "Normal",
            "Columns": [
              {
                "Layout": {
                  "FieldName": "CreatedDate",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              }
            ]
          },
          {
            "IsWrap": false,
            "GridRowType": "Normal",
            "Columns": [
              {
                "Layout": {
                  "FieldName": "SubmitButton",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              }
            ]
          }
        ],
        "IsBordered": false,
        "IsFlowLayout": false,
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
      }
    }
  },
  "ListLayouts": {
    "": {
      "Elements": [
        [
          { "FieldName": "Id", "Label": "ID", "CanResize": true, "CanUserSort": true },
          { "FieldName": "Name", "Label": "商品名", "CanResize": true, "CanUserSort": true },
          { "FieldName": "Price", "Label": "価格", "CanResize": true, "CanUserSort": true },
          { "FieldName": "IsActive", "Label": "有効", "CanResize": true, "CanUserSort": true },
          { "FieldName": "CreatedDate", "Label": "登録日", "CanResize": true, "CanUserSort": true }
        ]
      ]
    }
  },
  "SearchLayouts": {
    "": {
      "OnSearchInitialization": "",
      "ShowDefaultSearchButtons": true,
      "Layout": {
        "Operator": "And",
        "Rows": [
          {
            "IsWrap": false,
            "GridRowType": "Normal",
            "Columns": [
              {
                "Layout": {
                  "FieldName": "Name",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              },
              {
                "Layout": {
                  "FieldName": "IsActive",
                  "Name": "",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                },
                "Width": null
              }
            ]
          }
        ],
        "IsBordered": true,
        "IsExpandable": true,
        "ExpanderLabel": "検索条件",
        "IsExpanderDefaultOpened": false,
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
      }
    }
  },
  "LinkFieldNames": [],
  "ListPageFieldDesign": {
    "SearchCondition": {
      "LimitCount": 50,
      "ModuleName": ""
    },
    "CanDelete": true,
    "CanUserSort": true,
    "PagerPosition": "Bottom",
    "Name": "",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
  }
}
```

## LinkFieldNames の詳細

`LinkFieldNames` は、このモジュール内の LinkField が参照する先モジュールのフィールドを、自動的にロードして表示するための設定。

### 書式

```
"{LinkFieldのName}.{参照先モジュールのFieldName}"
```

- `LinkFieldのName`: このモジュール内の LinkFieldDesign の `Name`
- `参照先モジュールのFieldName`: LinkField が参照するモジュールのフィールドの `Name`

### 動作の仕組み

1. ユーザーが LinkField で値を選択すると、参照先モジュールのレコードが特定される
2. `LinkFieldNames` に列挙されたフィールドの値が、参照先モジュールから自動的にロードされる
3. ロードされた値は一覧画面や詳細画面で表示できる（読み取り専用）

### 実例

Recipe モジュールが Author（著者）への LinkField を持つ場合:

```json
{
  "Name": "Recipe",
  "Fields": [
    {
      "DbColumn": "author_id",
      "SearchCondition": {
        "ModuleName": "Author",
        "LimitCount": 50
      },
      "ValueVariable": "Id.Value",
      "DisplayTextVariable": "Fullname.Value",
      "Name": "Author",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
    }
  ],
  "LinkFieldNames": [
    "Author.Fullname",
    "Author.Email",
    "Author.Profile"
  ]
}
```

この設定により:
- Recipe のレコードを表示するとき、Author の Fullname, Email, Profile が自動ロードされる
- 一覧画面の ListElement で `"FieldName": "Author.Fullname"` のように参照できる
- 詳細画面の FieldLayoutDesign で `"FieldName": "Author.Fullname"` のように参照できる

### 一覧画面での使用例

```json
"ListLayouts": {
  "": {
    "Elements": [
      [
        { "FieldName": "Title", "Label": "レシピ名" },
        { "FieldName": "Author.Fullname", "Label": "著者名" },
        { "FieldName": "Author.Email", "Label": "著者メール" }
      ]
    ]
  }
}
```

### LinkFieldNames と IsRequired の関係

`LinkFieldNames` で参照されるフィールドはユーザーが直接編集するものではないため、`IsRequired: true` にしてはいけない。フィールドの用途が変わった（直接入力 → LinkFieldNames 経由の表示）場合は `IsRequired: false` に変更すること。

詳細は [CommonMistakes.md](CommonMistakes.md) の #19 を参照。

### 複数の LinkField を使う場合

```json
"LinkFieldNames": [
  "Product.Name",
  "Product.Code",
  "Client.Name",
  "Client.Phone",
  "User.Name"
]
```

各エントリの先頭部分が、このモジュール内の異なる LinkField 名に対応する。

---

## ListPageFieldDesign の役割

`ListPageFieldDesign` はモジュールが一覧ページとして表示される際のデフォルト ListField 設定。ただし、**PageFrame のリンク設定が優先される**。

- PageFrame (`*.frm.json`) の `ListPageDesign.ListFieldDesign` が設定されている場合、そちらが使われる
- PageFrame に設定がない場合のみ `ListPageFieldDesign` がフォールバックとして使われる

一覧ページのソート・件数制限・削除許可等は、**PageFrame 側で設定するのが正しいパターン**。

詳細は [PageFrame.md](PageFrame.md) および [CommonMistakes.md](CommonMistakes.md) の #18 を参照。

---

## 設計ポイント

1. **DB連携なし（表示専用）モジュール**: `DbTable` を空にすることで、DBと結びつかない表示専用モジュール（チャートダッシュボード、ダイアログ等）を作成可能。この場合:
   - `DataSourceName` は空文字にする
   - `IdFieldDesign` は不要（定義しない）
   - `SubmitButtonFieldDesign` は不要（定義しない）
   - `ListLayouts` の Elements にフィールドを入れないこと。フィールドを入れると一覧表示モジュールと認識されてしまう
2. **レイアウトの名前付き定義**: `DetailLayouts` / `ListLayouts` / `SearchLayouts` のキーに名前を付けることで、複数レイアウトを切り替え可能。`""` がデフォルト。
3. **LinkFieldNames**: LinkField を使って他モジュールのフィールドを表示する場合、そのパス（`"LinkFieldName.FieldName"`）をここに列挙する。上記「LinkFieldNames の詳細」を参照
4. **フィールドの TypeFullName**: 全フィールドに `TypeFullName` が必須。これによりデシリアライズ時に正しい型が復元される
