# SelectField - ドロップダウン選択

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign`

ドロップダウン形式の選択フィールド。静的な候補リスト、または別モジュールからの動的候補読み込みに対応する。
`DbValueFieldDesignBase` を継承し、選択された値をDBカラムに保存する。

## C# クラス定義 (真実の源)

```csharp
public class SelectFieldDesign : DbValueFieldDesignBase
{
    public List<string> Candidates { get; set; } = new();   // "表示テキスト,値" 形式の文字列配列
    public SearchCondition SearchCondition { get; set; } = new();
    public string ValueVariable { get; set; } = string.Empty;
    public string DisplayTextVariable { get; set; } = string.Empty;
    public override string DbColumn { get; set; } = string.Empty;
    public SelectEmptyCandidateType EmptyCandidateType { get; set; } = SelectEmptyCandidateType.StringEmpty;
    public bool AllowOrSearch { get; set; }
    // 親階層から継承: Name, IgnoreModification, OnValidateInput, DisplayName, IsRequired,
    //   OnDataChanged, IsUpdateProtected, IsSimpleSearchParameter, AllowEmptySearch, OnSearchDataChanged
}
```

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 選択値を保存するDBカラム名。 |
| `Candidates` | List\<string\> | `[]` | 静的候補リスト。`"表示テキスト,値"` の形式。カンマがない場合は表示テキスト＝値。例: `["未着手,1", "進行中,2", "完了,3"]` |
| `SearchCondition` | SearchCondition | `new()` | 動的候補を取得するための検索条件。`ModuleName` にソースモジュールを指定する。 |
| `ValueVariable` | string | `""` | ソースモジュール内の値の変数パス（例: `"Id.Value"`）。 |
| `DisplayTextVariable` | string | `""` | ソースモジュール内の表示テキストの変数パス（例: `"Name.Value"`）。 |
| `EmptyCandidateType` | SelectEmptyCandidateType | `"StringEmpty"` | 空選択肢の扱い。 |
| `AllowOrSearch` | bool | `false` | 検索時に複数選択値のOR検索を許可する。 |

## 列挙型

### SelectEmptyCandidateType

| 値 | 説明 |
|---|---|
| `StringEmpty` | 空文字列の選択肢を表示 |
| `Null` | null の選択肢を表示 |
| `NotExist` | 空選択肢を非表示（必ず値を選択） |

## JSON例

### 静的候補の場合

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
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "ValueVariable": "",
  "DisplayTextVariable": "",
  "EmptyCandidateType": "StringEmpty",
  "AllowOrSearch": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "ステータス",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Status",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign"
}
```

### 別モジュールから動的候補を取得する場合

```json
{
  "DbColumn": "category_code",
  "Candidates": [],
  "SearchCondition": {
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
  "ValueVariable": "Code.Value",
  "DisplayTextVariable": "Name.Value",
  "EmptyCandidateType": "NotExist",
  "AllowOrSearch": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "カテゴリ",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "Category_OnDataChanged",
  "Name": "Category",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign"
}
```

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Value` | string? | 選択された値 |
| `DisplayText` | string (読み取り専用) | 選択された値の表示テキスト |
| `DisplayTextAndValue` | Dictionary\<string, string\> (読み取り専用) | 表示テキスト→値のマップ |
| `SearchValue` | string? | 検索時の値 |
| `SearchValues` | IEnumerable\<string?\> | 検索時の複数値 |
| `AllowReloadLinkData` | bool | リンクデータ再読み込みを許可 |
| `AllowLoadCandidates` | bool | 候補読み込みを許可 |

### メソッド

| メソッド | 説明 |
|---|---|
| `SetCandidates(params string[] candidates)` | 候補を設定。`"表示,値"` 形式 |
| `SetCandidates(Dictionary<string, string> displayTextAndValue)` | 候補を辞書で設定 |
| `ReloadCandidates()` | 候補を再読み込み |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加検索条件を設定 |
| `SetNotFlag(bool value)` | 検索条件の反転フラグ |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ例

```csharp
// OnDataChanged: 選択変更時に連動フィールドを更新
void Category_OnDataChanged()
{
    // カテゴリ変更時にサブカテゴリの候補を更新
    var searcher = new ModuleSearcher<SubCategory>();
    searcher.AddEquals(e => e.CategoryId.Value, Category.Value);
    SubCategory.SetAdditionalCondition(searcher);
    SubCategory.ReloadCandidates();
}

// OnSearchDataChanged: 検索フォーム内での連動
void Status_OnSearchDataChanged()
{
    // ステータス選択に応じた絞り込み
    if (Status.SearchValue == "Completed")
    {
        CompletedDate.IsVisible = true;
    }
}
```

## ランタイム動作

- **静的候補:** `Candidates` リストからパースされる。各エントリは `"表示テキスト,値"` の形式で、カンマがなければ表示テキストと値が同一になる。
- **動的候補:** `SearchCondition.ModuleName` が設定されている場合、対象モジュールからデータを取得し、`ValueVariable` と `DisplayTextVariable` で値と表示テキストを取得する。フォーカス時に遅延読み込み（Lazy Load）する。
- **空選択肢:** `EmptyCandidateType` が `StringEmpty` なら空文字列の選択肢を表示、`Null` なら null の選択肢を表示、`NotExist` なら空選択肢を非表示にする。

## 検索

- 検索UIでは複数選択が可能。
- 複数値が選択された場合、各値に対してOR条件を生成する。
- `AllowOrSearch` が `true` の場合、OR検索が有効になる。
- `IsInverted`（検索条件の反転）が設定されるとNOT条件を生成する。

---

## DOM構造（CSS用）

### 編集モード

```html
<select class="form-select [is-invalid]" style="[インラインスタイル]">
  <option value="">（空選択肢）</option>
  <option value="値1">表示テキスト1</option>
  <option value="値2">表示テキスト2</option>
</select>
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">選択された表示テキスト</span>
```

### CSSセレクタ例

```css
/* セレクトボックスのスタイル */
[data-name="Status"] .form-select {
  max-width: 200px;
}

/* 表示モードの値 */
[data-name="Status"] .text {
  font-weight: bold;
}
```
