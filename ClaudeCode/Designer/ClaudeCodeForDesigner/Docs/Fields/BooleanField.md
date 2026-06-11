# BooleanField - 真偽値フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign`

真偽値（bool）を入力するフィールド。チェックボックス、トグルボタン、スイッチの3種類のUIスタイルから選択可能。表示専用モードでは true/false のカスタムテキストを表示できる。

## C# クラス定義 (真実の源)

```csharp
public class BooleanFieldDesign : DbValueFieldDesignBase, IDisplayName
{
    public string Text { get; set; } = "Boolean";
    public override string DbColumn { get; set; } = string.Empty;
    public BooleanUIType UIType { get; set; }                     // enum: CheckBox / ToggleButton / Switch
    public string TrueText { get; set; } = string.Empty;
    public string FalseText { get; set; } = string.Empty;
    // 親階層から継承 (詳細は _FieldCommon.md)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `Text` | string | `"Boolean"` | コントロールの横に表示されるラベルテキスト。 |
| `UIType` | BooleanUIType | `"CheckBox"` | 表示形式。 |
| `TrueText` | string | `""` | 表示専用モードで `true` のときに表示するカスタムテキスト。空の場合は `true` がそのまま表示される。 |
| `FalseText` | string | `""` | 表示専用モードで `false` のときに表示するカスタムテキスト。空の場合は `false` がそのまま表示される。 |

## 列挙型

### BooleanUIType

| 値 | 説明 |
|---|---|
| `CheckBox` | チェックボックス |
| `ToggleButton` | トグルボタン |
| `Switch` | スイッチ |

## JSON例

### 基本的なチェックボックス（有効フラグ）

```json
{
  "DbColumn": "is_active",
  "Text": "有効",
  "UIType": "CheckBox",
  "TrueText": "",
  "FalseText": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "有効フラグ",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "IsActive",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```

### スイッチUI（公開設定）

```json
{
  "DbColumn": "is_published",
  "Text": "公開する",
  "UIType": "Switch",
  "TrueText": "公開中",
  "FalseText": "非公開",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": true,
  "OnSearchDataChanged": "",
  "DisplayName": "公開設定",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "IsPublished_OnDataChanged",
  "Name": "IsPublished",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```

### トグルボタン（承認状態）

```json
{
  "DbColumn": "is_approved",
  "Text": "承認済み",
  "UIType": "ToggleButton",
  "TrueText": "承認済",
  "FalseText": "未承認",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "承認状態",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "IsApproved",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```

## ランタイム動作

- **UIType による表示の違い:**
  - `CheckBox`: 標準のHTMLチェックボックスとして表示される。
  - `ToggleButton`: ボタンスタイルのトグルとして表示される。押下状態で true/false を切り替える。
  - `Switch`: スイッチ（スライドトグル）UIとして表示される。
- **Text:** いずれのUITypeでも、コントロールの横にラベルテキストとして表示される。
- **TrueText / FalseText:** 表示専用モード（閲覧時）のみ使用される。
  - `TrueText` が空の場合、true のときは生の `true` が表示される。
  - `FalseText` が空の場合、false のときは生の `false` が表示される。
  - 設定例: `TrueText = "有効"`, `FalseText = "無効"` とすれば、閲覧時に分かりやすいテキストが表示される。
- **編集モード:** UIType に応じたコントロールが表示され、クリック/タップで値を切り替える。

## 検索

- `Equal` 比較による検索をサポートする。
- 検索UIでは nullable bool（null / true / false）の3状態フィルタが表示される。
  - `null`: フィルタなし（全件表示）。
  - `true`: true のレコードのみ表示。
  - `false`: false のレコードのみ表示。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | bool? | 読み書き | 真偽値 |
| `SearchValue` | bool? | 読み書き | 検索時の値（null=フィルタなし、true=trueのみ、false=falseのみ） |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### CheckBox（UIType = "CheckBox"）

```html
<div class="form-check [is-invalid]" style="[インラインスタイル]">
  <input type="checkbox" class="form-check-input cursor-pointer [is-invalid]" id="..." />
  <label class="form-check-label user-select-none cursor-pointer" for="...">テキスト</label>
</div>
<div class="invalid-feedback">エラーメッセージ</div>
```

### Switch（UIType = "Switch"）

```html
<div class="form-check form-switch [is-invalid]" style="[インラインスタイル]">
  <input type="checkbox" class="form-check-input cursor-pointer [is-invalid]" id="..." />
  <label class="form-check-label user-select-none cursor-pointer" for="...">テキスト</label>
</div>
<div class="invalid-feedback">エラーメッセージ</div>
```

### ToggleButton（UIType = "ToggleButton"）

```html
<button class="btn btn-[variant] [active]" style="[インラインスタイル]">テキスト</button>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">TrueText/FalseText</span>
```

### CSSセレクタ例

```css
/* チェックボックスのサイズ変更 */
[data-name="IsActive"] .form-check-input {
  width: 1.5em;
  height: 1.5em;
}

/* スイッチのアクセントカラー */
[data-name="IsEnabled"] .form-switch .form-check-input:checked {
  background-color: #198754;
  border-color: #198754;
}
```
