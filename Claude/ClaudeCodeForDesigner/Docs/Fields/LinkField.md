# LinkField - 外部キーリンク

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign`

他モジュールへの外部キーリンクフィールド。検索ダイアログから関連レコードを選択し、外部キー値を保存する。
`DbValueFieldDesignBase` を継承する。

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 自テーブルの外部キーカラム名。 |
| `SearchCondition` | SearchCondition | `new()` (LimitCount=50) | リンク先モジュールの検索条件。`ModuleName` にリンク先モジュールを設定する。 |
| `ValueVariable` | string | `""` | リンク先モジュール内のID変数パス（例: `"Id.Value"`）。選択時にこの値が `DbColumn` に保存される。 |
| `DisplayTextVariable` | string | `""` | リンク先モジュール内の表示テキスト変数パス（例: `"Name.Value"`）。UIに表示する文字列。 |
| `ListLayoutName` | string | `""` | 検索ダイアログで使用する一覧レイアウト名。空の場合はデフォルトレイアウト。 |
| `SearchLayoutName` | string | `""` | 検索ダイアログで使用する検索レイアウト名。空の場合はデフォルトレイアウト。 |
| `OnSearchButtonClicked` | string | `""` | 検索ボタンクリック時のスクリプトイベント名。設定するとデフォルトのモーダル表示の代わりにカスタムロジックを実行する。 |

## JSON例

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
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "ValueVariable": "Id.Value",
  "DisplayTextVariable": "Name.Value",
  "ListLayoutName": "",
  "SearchLayoutName": "",
  "OnSearchButtonClicked": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "カテゴリ",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Category",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
}
```

### フィルタ条件付きの例

```json
{
  "DbColumn": "department_id",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Department",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "IsActive.Value",
          "Comparison": "Equal",
          "Value": { "Value": "True" },
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "ValueVariable": "Id.Value",
  "DisplayTextVariable": "DepartmentName.Value",
  "ListLayoutName": "SelectList",
  "SearchLayoutName": "SelectSearch",
  "OnSearchButtonClicked": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "DisplayName": "部署",
  "IsRequired": false,
  "Name": "Department",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
}
```

## 変数パスに関する注意

LinkField の外部キー値（DbColumn に保存されている値）を参照する場合は **`LinkFieldName.Value`** を使う。

**よくある間違い:** `LinkFieldName.Id.Value` と書くのは **誤り**。LinkField は外部キー値を直接保持しているため、`.Value` でアクセスする。

```
✅ 正しい: ExpenseReportId.Value      （LinkField 自体の値 = 外部キー）
❌ 誤り:   ExpenseReportId.Id.Value   （存在しないパス）
```

これは以下すべてに共通する:
- **SearchCondition** の `SearchTargetVariable` / `Variable`
- **スクリプト** でのフィールドアクセス（`field.Value`）
- **ModuleSearcher** のラムダ式（`e => e.LinkFieldName.Value`）

> `LinkFieldName.ReferenceFieldName.Value` の形式は、リンク先モジュールの **別のフィールド** を参照する場合にのみ使う（例: `Category.Name.Value` でリンク先の Name フィールドを取得）。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Value` | string? | リンク先のID値（= DbColumn に保存されている外部キー値） |
| `DisplayText` | string | リンク先の表示テキスト |
| `SearchValue` | string? | 検索時の値 |
| `AllowReloadLinkData` | bool | リンクデータ再読み込みを許可 |

### メソッド

| メソッド | 説明 |
|---|---|
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加検索条件を設定 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ例

```csharp
// OnSearchButtonClicked: カスタム検索ダイアログ
void Category_OnSearchButtonClicked()
{
    var dlg = new CategorySelector();
    var result = dlg.ShowDialog("選択", "キャンセル");
    if (result == "選択")
    {
        Category.Value = dlg.SelectedId.Value;
    }
}
```

## ランタイム動作

- UIに検索ボタンが表示される。クリックするとリンク先モジュールの一覧をモーダルダイアログで表示する。
- ユーザーがレコードを選択すると、`ValueVariable` で指定された値が `DbColumn` に保存される。
- `DisplayTextVariable` で指定された値がUI上の表示テキストとして使用される。
- `GetAndSetLinkDataAsync()` により、リンク先レコードのデータを取得し、子リンクフィールドに展開する。
- `OnSearchButtonClicked` が設定されている場合、デフォルトのモーダル表示の代わりにスクリプトイベントが実行される。

## 検索

- `ValueVariable` に対する Equal 比較で検索を行う。

---

## DOM構造（CSS用）

### 編集モード

```html
<div class="input-group [is-invalid]">
  <input type="text" class="form-control" readonly value="表示テキスト" style="[インラインスタイル]" />
  <button class="btn btn-outline-secondary" data-system="search" type="button">
    <span class="oi oi-magnifying-glass" aria-hidden="true"></span>
  </button>
</div>
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">表示テキスト</span>
```

### CSSセレクタ例

```css
/* リンクフィールドの検索ボタン */
[data-name="Category"] .btn-outline-secondary {
  border-color: #0d6efd;
  color: #0d6efd;
}

/* 読み取り専用の入力部分 */
[data-name="Category"] .form-control[readonly] {
  background-color: #f8f9fa;
}
```
