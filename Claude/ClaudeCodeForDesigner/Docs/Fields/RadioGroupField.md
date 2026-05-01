# RadioGroupField / RadioButtonField - ラジオボタングループ

## RadioGroupFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.RadioGroupFieldDesign`

ラジオボタングループの親フィールド。選択された値を保持し、DBカラムにマッピングする。
`DbValueFieldDesignBase` を継承する。

## RadioButtonFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign`

ラジオボタンの個別選択肢。`FieldDesignBase` を直接継承する（値は親の RadioGroupField が保持するため）。

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

### RadioGroupField のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 選択値を保存するDBカラム名。 |
| `AllowOrSearch` | bool | `false` | 検索時に複数選択値のOR検索を許可する。 |
| `PopulateRadioButtons` | bool | `false` | `true` にすると、DBの既存値からラジオボタンを自動生成する。 |

### RadioButtonField のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | string | `"Radio"` | この選択肢の表示ラベル。 |
| `Value` | string | `""` | この選択肢が選ばれた時の値。 |
| `GroupField` | string | `""` | 所属する RadioGroupField の `Name`。 |

## JSON例

### RadioGroupField

```json
{
  "DbColumn": "priority",
  "AllowOrSearch": false,
  "PopulateRadioButtons": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "優先度",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Priority",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioGroupFieldDesign"
}
```

### RadioButtonField（各選択肢）

```json
{
  "Text": "高",
  "Value": "High",
  "GroupField": "Priority",
  "Name": "PriorityHigh",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
}
```

```json
{
  "Text": "中",
  "Value": "Medium",
  "GroupField": "Priority",
  "Name": "PriorityMedium",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
}
```

```json
{
  "Text": "低",
  "Value": "Low",
  "GroupField": "Priority",
  "Name": "PriorityLow",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
}
```

### Fields 配列での配置例

```json
"Fields": [
  {
    "DbColumn": "priority",
    "AllowOrSearch": false,
    "PopulateRadioButtons": false,
    "DisplayName": "優先度",
    "Name": "Priority",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioGroupFieldDesign"
  },
  {
    "Text": "高",
    "Value": "High",
    "GroupField": "Priority",
    "Name": "PriorityHigh",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
  },
  {
    "Text": "中",
    "Value": "Medium",
    "GroupField": "Priority",
    "Name": "PriorityMedium",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
  },
  {
    "Text": "低",
    "Value": "Low",
    "GroupField": "Priority",
    "Name": "PriorityLow",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
  }
]
```

## ランタイム動作

- RadioButtonField をクリックすると、その `Value` が親の RadioGroupField の値として設定される。
- 表示は各 RadioButtonField の `Text` がラベルとして表示される。
- RadioGroupField が値を保持し、DB への読み書きを行う。RadioButtonField 自体はデータを持たない。
- `PopulateRadioButtons` が `true` の場合、一覧表示時にDBの既存値からラジオボタンを自動生成する。

## 検索

- SelectField と同様に、複数値のOR検索に対応する。
- `AllowOrSearch` が `true` の場合、複数の RadioButton を選択してOR条件を生成できる。

## 重要な注意事項

- RadioButtonField は、Fields 配列内で必ず対応する RadioGroupField の**後**に定義すること。
- RadioButtonField の `GroupField` は、必ず同一モジュール内に存在する RadioGroupField の `Name` を参照すること。
- レイアウトには RadioGroupField と RadioButtonField の両方を配置する必要がある。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | string? | 読み書き | 選択された値 |
| `DisplayText` | string | 読み取り | 選択された値の表示テキスト |
| `SearchValue` | string? | 読み書き | 検索時の値 |
| `SearchValues` | IEnumerable\<string?\> | 読み書き | 検索時の複数値 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### RadioGroupField（コンテナ）

RadioGroupField 自体はDOMを出力しない。各 RadioButtonField が個別にレイアウトに配置され、以下のDOMを出力する。

### RadioButtonField（各ボタン）

#### 編集モード

```html
<div class="form-radio [is-invalid]" style="[インラインスタイル]">
  <input type="radio" class="form-check-input cursor-pointer [is-invalid]" id="..." />
  <label class="form-check-label user-select-none cursor-pointer" for="...">選択肢テキスト</label>
</div>
<div class="invalid-feedback">エラーメッセージ</div>
```

#### 表示モード（一覧）

```html
<span class="d-block py-2 text" style="[インラインスタイル]">選択値</span>
```

### CSSセレクタ例

```css
/* ラジオボタンのサイズ */
[data-name="PriorityHigh"] .form-check-input {
  width: 1.25em;
  height: 1.25em;
}

/* ラジオボタンのラベル */
[data-name="PriorityHigh"] .form-check-label {
  font-weight: 600;
  color: #dc3545;
}
```
