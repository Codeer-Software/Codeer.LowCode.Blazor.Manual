# モジュール定義 (*.mod.json)

モジュールはアプリケーションの画面・データ定義の中核となるファイルである。1つのモジュールが1つの画面（ページ/フォーム）に対応し、フィールド定義、レイアウト、CRUD権限、検索条件等を含む。

## C# クラス定義 (真実の源)

ソースコード `Source/Codeer.LowCode.Blazor/Repository/Design/ModuleDesign.cs` から転記。JSON のスキーマの **唯一の真実**。プロパティの型 (`List<T>` / `Dictionary<K,V>` / `T?`) や初期値で迷ったら必ずここを見る。`[Obsolete]` 付きのプロパティは新規作成 JSON で書かなくてよい (互換のため出力されることはある)。

```csharp
public class ModuleDesign
{
    public string Name { get; set; } = string.Empty;
    public string DataSourceName { get; set; } = string.Empty;
    public string DbTable { get; set; } = string.Empty;
    public bool CanCreate { get; set; } = true;
    public bool CanUpdate { get; set; } = true;
    public bool CanDelete { get; set; } = true;

    public List<FieldDesignBase> Fields { get; set; } = new();   // 配列。各要素 TypeFullName 必須

    public ModuleMatchCondition UserWriteCondition { get; set; } = new();
    public ModuleMatchCondition UserReadCondition { get; set; } = new();
    public ModuleMatchCondition DataWriteCondition { get; set; } = new();
    public ModuleMatchCondition DataReadCondition { get; set; } = new();

    public Dictionary<string, DetailLayoutDesign> DetailLayouts { get; set; }   // {"": {...}} (object)
        = new() { { string.Empty, new() } };
    public Dictionary<string, ListLayoutDesign>   ListLayouts   { get; set; }   // {"": {...}}
        = new() { { string.Empty, new() } };
    public Dictionary<string, SearchLayoutDesign> SearchLayouts { get; set; }   // {"": {...}}
        = new() { { string.Empty, SearchLayoutDesign.CreateFirstSearchLayout() } };

    public List<string> LinkFieldNames { get; set; } = new();   // 配列

    [Obsolete] public bool UseListPageSubmitButton { get; set; }
    [Obsolete] public FieldDesignBase ListPageFieldDesign { get; set; }
        = ListPageDesign.CreateDefaultListPageFieldDesign();
    [Obsolete] public bool CanBulkDataUpdate { get; set; }
    [Obsolete] public bool CanBulkDataDownload { get; set; }
    [Obsolete] public string PageTitle { get; set; } = string.Empty;
}
```

> **注意 (Claude 向け)**:
> - `Fields` は `List<FieldDesignBase>` (= JSON 配列)。各要素は `TypeFullName` 必須 (`LabelFieldDesign` / `TextFieldDesign` 等のサブクラス)。
> - `DetailLayouts` / `ListLayouts` / `SearchLayouts` は `Dictionary<string, ...>` (= JSON オブジェクト `{"": {...}}`)。`""` キーがデフォルト。**配列で書くと NG**。
> - `LinkFieldNames` は `List<string>` (= JSON 配列)。
> - `[Obsolete]` 5 つは新規モジュールに書かない (PageFrame の ListPageDesign 側に移行済み)。古い mod.json には残っていることがあるが触らないで OK。

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

### 名前の一意性（デザインチェックでエラー）

「型として扱われる名前」は **種別を跨いで一意** でなければならない。次がすべて同じ名前空間を共有する:

- モジュール名 / ページフレーム名
- フィールドの型名（`TextField` `NumberField` 等）/ スクリプトで使える型・サービス名（`DateTime` `Math` `List` `Dictionary` `Logger` `MessageBox` 等）
- ProCodeComponent / ProCodeModule / ListElementComponent のクラス名

ルール:

- **重複禁止**: モジュール名・ページフレーム名を、他のモジュール/ページフレーム/上記のいずれかと同じ名前にしない（例: モジュール名 `DateTime` や `TextField`、`List` は不可）。
- **大文字小文字は区別しない**: `Order` と `order` は重複扱い（設計はファイル `{Name}.mod.json` で保存され、Windows のファイルシステムも大小無視のため、そもそも共存できない）。
- **C# の識別子として有効**にする: 先頭は文字か `_`、以降は文字・数字・`_`。日本語などの Unicode 文字は可。`.`・空白・`-` や C# キーワード（`if` `class` `int` 等）は不可。
- **ProCodeBehind は対象外**: コードビハインドのクラス名は対応するモジュール名と一致させる前提なので、重複として扱わない（むしろ一致させる）。

これらに反するとデザインチェックでエラーになる。

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

---

## システムフィールド（予約名・型が固定）

以下の `Name` は CLB の予約名で型が決まっている。この綴りの `Name` にすると CLB が自動で振る舞う（主キー / 論理削除フィルタ / 時刻自動セット / ユーザー自動セット / 楽観ロック）。指示がない限り変更しない。`DbColumn` は任意の列名でよい。

| Name | 型 | 説明 |
|---|---|---|
| Id | IdFieldDesign | 主キー |
| LogicalDelete | BooleanFieldDesign | 論理削除フラグ |
| CreatedAt | DateTimeFieldDesign | 作成日時（保存時に自動セット） |
| UpdatedAt | DateTimeFieldDesign | 更新日時（保存時に自動セット） |
| Creator | LinkFieldDesign | 作成者（保存時に自動セット） |
| Updater | LinkFieldDesign | 更新者（保存時に自動セット） |
| OptimisticLocking | OptimisticLockingFieldDesign | 楽観ロック |

### OptimisticLocking（楽観ロック）の設定

`OptimisticLockingFieldDesign` の重要プロパティ:
- `DbColumn`: バージョンを保存する列名
- `IncrementVersion`: アプリ（ソフト）側でバージョンを増やすか

モードは2通り:

1. **アプリ（ソフト）でインクリメント**（一般的）: `IncrementVersion: true`。保存のたびにアプリがバージョン番号を +1 する。
   この方式は**バージョンを保存する整数列が必須**なので、`DbColumn` に列名（snake_case、例 `optimistic_locking`）を**必ず設定する**。その列は DB に追加が必要（整数型。`Create DDL` で追加できる）。
   → 「アプリ/ソフトでインクリメント」「カラム名を入れて」と言われたら **`IncrementVersion: true` かつ `DbColumn: "optimistic_locking"`**。`DbColumn` を空にしてはいけない。

2. **DB のネイティブ行バージョン**（PostgreSQL の `xmin` 等）: `IncrementVersion: false`。`DbColumn` に既存のネイティブ版管理列を指定する。新しい列の追加は不要。

**よくある誤り**: 「楽観ロックは DB 非依存だから `DbColumn` は空」は**誤り**。アプリインクリメント方式では `DbColumn`（整数列）が必要。指示で `DbColumn` を求められたら必ず列名を入れること。

**プロパティはこれだけ**: `OptimisticLockingFieldDesign` が持つのは `Name` / `DbColumn` / `IncrementVersion`（＋共通の `IgnoreModification` / `OnValidateInput`）のみ。**`DisplayName` や `IsRequired` などの値フィールド用プロパティは持たない**ので付けないこと（存在しないプロパティを書くとエラーになり、設定全体が適用されません）。`DisplayName` 等の共通プロパティを持つのは値フィールド（`ValueFieldDesignBase` 派生＝Text/Number/Boolean/Date/Select/Link 等）だけ。Button/Label 以外の非値フィールドに共通プロパティを付けないこと。

---

## フィールド型カタログ（用途早見）

`Fields` 配列に入れるフィールド型の早見。各型の詳細プロパティは [Fields/](Fields/) の各ドキュメントを参照。

### 入力系（DBカラムと紐づく）
- IdFieldDesign(主キー) / TextFieldDesign(テキスト) / NumberFieldDesign(数値) / BooleanFieldDesign(真偽) / DateFieldDesign(日付) / DateTimeFieldDesign(日時) / TimeFieldDesign(時刻) / PasswordFieldDesign(パスワード,DB列なし)
- SelectFieldDesign(ドロップダウン。Candidates "表示,値" または SearchCondition+ValueVariable+DisplayTextVariable)
- RadioGroupFieldDesign + RadioButtonFieldDesign(ラジオ。RadioButtonのGroupFieldにRadioGroupのNameを指定)
- LinkFieldDesign(外部キー。SearchCondition.ModuleNameで参照先、ValueVariable/DisplayTextVariableで表示)
- FileFieldDesign(ファイル) / JsonFieldDesign(JSON構造化データ)

### 表示系（DB列なし）
- LabelFieldDesign(ラベル) / ImageViewerFieldDesign(画像) / MarkupStringFieldDesign(HTML) / AnchorTagFieldDesign(リンク/遷移)

### ボタン系（DB列なし）
- ButtonFieldDesign(汎用,OnClick) / SubmitButtonFieldDesign(保存) / CopyModuleButtonFieldDesign(コピー) / ViewEditToggleButtonFieldDesign(表示/編集切替)

### 一覧・構造系
- ListFieldDesign(表形式一覧) / DetailListFieldDesign(明細インライン編集) / TileListFieldDesign(タイル) / ListPagingFieldDesign(ページング) / ListNumberFieldDesign(行番号) / SearchFieldDesign(検索フォーム) / ModuleFieldDesign(他モジュール埋め込み)

**ListField と DetailListField の選び方（最頻出の誤り）**: 「リストを追加」「一覧で表示」「明細を表示」のような“他モジュールのレコードを並べる”依頼は、既定で **`ListFieldDesign`（表形式・列が揃った表。子モジュールの `ListLayouts` で列を描く）** を使う。`DetailListFieldDesign` は「各行を子モジュールの**詳細レイアウト（フォーム/カード）**でインライン編集する」特殊な並べ方で、明確に「各レコードを 1 枚のフォーム/カードにして並べたい」「その場でフォーム編集したい」と判断したときだけ選ぶ。
- **参照先モジュール名に「明細」が含まれていても、それを理由に `DetailListFieldDesign` を選ばない。** 例:「リストを追加して、モジュールは請求書明細を使って」「明細を追加して、モジュールは請求書明細を使って」→ 求められているのは **`ListFieldDesign`**（請求書明細モジュールを参照する表形式一覧）。`請求書明細` は**モジュール名**であって、フィールド型の指定ではない。"DetailList" の "Detail" は「詳細レイアウトで描く」の意味で、業務用語の「明細行」ではない。
- **迷ったら `ListFieldDesign`。**

### メニュー系（DB列なし）
- HeaderMenuFieldDesign / SidebarMenuFieldDesign / ContextMenuFieldDesign

### チャート系（外部ライブラリ, DB列なし）
- ApexChartFieldDesign(棒/折れ線/面/散布/ヒートマップ) / ApexRadialChartFieldDesign(円/ドーナツ/極座標)

### その他
- OptimisticLockingFieldDesign(楽観ロック) / ProCodeFieldDesign(カスタムBlazorコンポーネント)

---

## 型固有プロパティ早見（作成・編集時に設定する主なもの）

ユーザーが「最大長」「小数桁」「複数行」「候補」等を求めたら、対応する**型固有プロパティを必ず設定する**こと（共通基底の DisplayName / IsRequired 等は [_FieldCommon] 参照）。現在 null のプロパティでも、その型に存在するものは設定してよい。

| 型 | 主な型固有プロパティ |
|---|---|
| TextFieldDesign | `MaxLength`(最大長, int?) / `IsMultiline`(複数行, bool) / `Rows`(複数行の行数, int?) / `Placeholder`(プレースホルダ) |
| NumberFieldDesign | `MaxFractionDigits`(小数桁数, int?。「小数2桁」なら 2) / `Max`(最大値, decimal?) / `IsSlider`(スライダ) / `Format`(書式) |
| DateFieldDesign / DateTimeFieldDesign / TimeFieldDesign | `SaveAsUtc`(UTC保存, bool。CreatedAt/UpdatedAt 等は true) |
| SelectFieldDesign | `Candidates`(List<string>。**`"表示,値"` の順**。例 `"有効,1"`) ※ または SearchCondition + ValueVariable + DisplayTextVariable |
| RadioGroupFieldDesign + RadioButtonFieldDesign | RadioButton 側に `GroupField`(= RadioGroup の Name) / `Text`(表示) / `Value`(値) を設定 |
| LinkFieldDesign | `SearchCondition.ModuleName`(参照先モジュール) / `ValueVariable`(値の変数) / `DisplayTextVariable`(表示の変数) |
| FileFieldDesign | `DbColumnFileGuid` / `DbColumnFileName` / `DbColumnFileSize`（3列。単一の DbColumn ではない） |
| AnchorTagFieldDesign | `Target`(遷移種別) / `Url` / `Module` / `Icon` / `TitleText` |
| ButtonFieldDesign | `OnClick`(スクリプト) / `Label` |
| OptimisticLockingFieldDesign | `DbColumn` / `IncrementVersion`（[システムフィールド] の楽観ロック参照） |

詳細プロパティは各型の定義に従う。ここに無い型固有プロパティも、現在のモジュール設定JSON（null 含む全プロパティが見える）から名前を確認して設定できる。
