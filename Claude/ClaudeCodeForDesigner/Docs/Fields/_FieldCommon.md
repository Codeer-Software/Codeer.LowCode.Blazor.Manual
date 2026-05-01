# フィールド共通プロパティ

全フィールド型は以下の基底クラスの階層を持つ。フィールド型により継承する基底が異なる。

## 継承階層

```
FieldDesignBase                         ← 全フィールド共通
├── ValueFieldDesignBase                ← 値を持つフィールド
│   ├── DbValueFieldDesignBase          ← DB列にマッピングされるフィールド
│   │   ├── TextFieldDesign, NumberFieldDesign, BooleanFieldDesign, ...
│   │   ├── SelectFieldDesign, LinkFieldDesign, RadioGroupFieldDesign, ...
│   │   └── IdFieldDesign, DateFieldDesign, DateTimeFieldDesign, TimeFieldDesign, JsonFieldDesign
│   └── PasswordFieldDesign
├── ListFieldDesignBase                 ← 一覧系フィールド
│   ├── ListFieldDesign
│   ├── DetailListFieldDesign
│   └── TileListFieldDesign
├── ButtonFieldDesign, SubmitButtonFieldDesign, CopyModuleButtonFieldDesign
├── LabelFieldDesign, ImageViewerFieldDesign, MarkupStringFieldDesign
├── AnchorTagFieldDesign, ContextMenuFieldDesign
├── SearchFieldDesign, ModuleFieldDesign
├── FileFieldDesign, OptimisticLockingFieldDesign
├── QueryFieldDesign, ExecuteSqlFieldDesign
├── HeaderMenuFieldDesign, SidebarMenuFieldDesign
├── ListNumberFieldDesign, ListPagingFieldDesign
├── ProCodeFieldDesign, RadioButtonFieldDesign
└── ViewEditToggleButtonFieldDesign
```

## FieldDesignBase （全フィールド共通）

全てのフィールドが持つプロパティ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | `""` | フィールド名。モジュール内で一意。PascalCase推奨。レイアウトやスクリプトからこの名前で参照する。 |
| `IgnoreModification` | bool | `false` | `true` にすると、このフィールドの変更を変更追跡から除外する。Submit時に変更として扱われない。 |
| `TypeFullName` | string | （自動） | JSON シリアライズ時に型を識別するための完全修飾名。各フィールド型のドキュメントに記載。 |

**JSONでの記述:**
```json
{
  "Name": "FieldName",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

## ValueFieldDesignBase （値フィールド共通）

値を保持するフィールドの共通プロパティ。FieldDesignBase のプロパティも全て継承。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 画面に表示されるラベル名。空の場合は `Name` が使われる。 |
| `IsRequired` | bool | `false` | `true` にすると入力必須。ValidateInput()で空値チェックされ、エラーが表示される。 |
| `OnDataChanged` | string | `""` | フィールドの値が変更された時に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |

## DbValueFieldDesignBase （DBフィールド共通）

データベース列にマッピングされるフィールドの共通プロパティ。ValueFieldDesignBase のプロパティも全て継承。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDBテーブル列名。snake_case推奨。空の場合はDB連携しない。 |
| `IsUpdateProtected` | bool | `false` | `true` にすると、レコード作成後は値を変更できなくなる（読み取り専用になる）。新規作成時は編集可能。 |
| `IsSimpleSearchParameter` | bool | `false` | `true` にすると、簡易検索パラメータとして自動的に検索条件に含まれる。 |
| `OnSearchDataChanged` | string | `""` | 検索値が変更された時に呼ばれるスクリプトイベント名。 |

### IsUpdateProtected の用途

`IsUpdateProtected` 読み取り専用になるプロパティ。以下のようなフィールドに適する:

- DBから値を取得するだけ（例：他の処理で書き換えるが、このモジュールでは変更しない）

```json
{
  "DbColumn": "code",
  "IsUpdateProtected": true,
  "Name": "Code",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

**IsViewOnly との違い:** `IsViewOnly` は表示上読み取り専用。`IsUpdateProtected` はサーバーサイドでもチェックが入るので、API経由での更新も防止される。

### IsSimpleSearchParameter の用途

`IsSimpleSearchParameter: true` を設定すると、SearchField を使わなくても一覧ページの簡易検索バーにこのフィールドが含まれる。

```json
{
  "DbColumn": "name",
  "IsSimpleSearchParameter": true,
  "Name": "Name",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

## ListFieldDesignBase （一覧フィールド共通）

ListFieldDesign, DetailListFieldDesign, TileListFieldDesign の共通プロパティ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 一覧の表示名。 |
| `SearchCondition` | SearchCondition | `new()` | データ取得条件。対象モジュール名、フィルタ条件、ソート、件数制限を定義。 |
| `LayoutName` | string | `""` | 使用するレイアウト名。空の場合はデフォルトレイアウト。 |
| `PagerPosition` | PagerPosition | `"Top"` | ページャーの位置。`Top` / `Bottom` / `Both`。 |
| `UseIndexSort` | bool | `false` | `true` で NumberField によるインデックスソートを使用。 |
| `DeleteTogether` | bool | `false` | `true` で親レコード削除時に子レコードも一括削除。 |
| `CanCreate` | bool | `false` | 行の新規作成を許可するか。 |
| `CanUpdate` | bool | `false` | 行の更新を許可するか。 |
| `CanDelete` | bool | `false` | 行の削除を許可するか。 |
| `CanUserSort` | bool | `true` | ユーザーによる列ヘッダークリックでのソートを許可するか。 |
| `CanSelect` | bool | `false` | 行の選択UIを表示するか。 |
| `OnDataChanged` | string | `""` | データ変更時のスクリプトイベント名。 |
| `OnSearchDataChanged` | string | `""` | 検索データ変更時のスクリプトイベント名。 |
| `OnSelectedIndexChanged` | string | `""` | 行選択変更時のスクリプトイベント名。 |
| `OnSelectedIndexChanging` | string | `""` | 行選択変更前のスクリプトイベント名。`false` を返すと選択をキャンセルできる。 |
| `OnDoubleClickRow` | string | `""` | 行ダブルクリック時のスクリプトイベント名。 |

## スクリプトイベントハンドラの記述例

### OnDataChanged（ValueFieldDesignBase）

値フィールドの `OnDataChanged` プロパティに指定したメソッド名が、フィールドの値変更時に呼ばれる。引数なし。

```csharp
// mod.json: { "OnDataChanged": "Price_OnDataChanged", "Name": "Price" }
void Price_OnDataChanged()
{
    Tax.Value = Math.Round(Price.Value * 0.1, 0, MidpointRounding.AwayFromZero);
    TotalPrice.Value = Price.Value + Tax.Value;
}
```

### OnSearchDataChanged（DbValueFieldDesignBase）

DBフィールドの `OnSearchDataChanged` プロパティに指定したメソッド名が、検索フォーム内でフィールド値が変更された時に呼ばれる。引数なし。

```csharp
// mod.json: { "OnSearchDataChanged": "Category_OnSearchDataChanged", "Name": "Category" }
void Category_OnSearchDataChanged()
{
    // カテゴリ変更時にサブカテゴリの候補を更新
    var searcher = new ModuleSearcher<SubCategory>();
    searcher.AddEquals(e => e.CategoryId.Value, Category.Value);
    SubCategory.SetAdditionalCondition(searcher);
    SubCategory.ReloadCandidates();
}
```

### ListFieldDesignBase のイベントハンドラ

一覧系フィールド（ListField, DetailListField, TileListField）のイベント詳細は [ListField.md](ListField.md) を参照。

---

## フィールドの分類

### DB列マッピングあり（データ永続化）
IdField, TextField, NumberField, BooleanField, DateField, DateTimeField, TimeField,
SelectField, LinkField, RadioGroupField, JsonField

### DB列マッピングなし（UI/ロジック用）
ButtonField, SubmitButtonField, CopyModuleButtonField, LabelField, ImageViewerField,
MarkupStringField, AnchorTagField, SearchField, ContextMenuField, ListNumberField,
ListPagingField, HeaderMenuField, SidebarMenuField, ViewEditToggleButtonField

### 特殊（複合データ）
FileField（3列: FileName, FileSize, FileGuid）, ModuleField（子モジュール）,
OptimisticLockingField（楽観ロック用）

### 一覧系
ListField, DetailListField, TileListField

### クエリ/SQL実行
QueryField, ExecuteSqlField
