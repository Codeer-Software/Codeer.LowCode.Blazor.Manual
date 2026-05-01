# IdField - 主キーフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign`

レコードの主キーとなるフィールド。手動入力モードでなければ UUID を自動生成する。複合キーにも対応しており、他フィールドの値をセパレータで結合してIDを構成できる。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 主キー列名。snake_case推奨（例: `id`）。 |
| `Placeholder` | string | `""` | 手動入力モード時のプレースホルダーテキスト。 |
| `IsManualInput` | bool | `false` | `true` の場合、ユーザーがIDを手動入力する。`false` の場合、UUID が自動生成される。 |
| `CompositeIdVariables` | List\<string\> | `[]` | 複合キーを構成するフィールド名のリスト。指定すると各フィールドの値を結合してIDを生成する。 |
| `CompositeIdSeparator` | string | `""` | 複合キーの各パーツを結合するセパレータ文字列（例: `"-"`, `"_"`）。 |
| `SearchComparisonDefaultValue` | MatchComparison? | `null` | 検索時のデフォルト比較演算子。`Equal` または `Like` を指定可能。 |

## JSON例

### 基本的なUUID自動生成の主キー

```json
{
  "DbColumn": "id",
  "Placeholder": "",
  "IsManualInput": false,
  "CompositeIdVariables": [],
  "CompositeIdSeparator": "",
  "SearchComparisonDefaultValue": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Id",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
}
```

### 手動入力IDフィールド（社員番号など）

```json
{
  "DbColumn": "employee_code",
  "Placeholder": "例: EMP-001",
  "IsManualInput": true,
  "CompositeIdVariables": [],
  "CompositeIdSeparator": "",
  "SearchComparisonDefaultValue": "Like",
  "IsUpdateProtected": true,
  "IsSimpleSearchParameter": true,
  "OnSearchDataChanged": "",
  "DisplayName": "社員コード",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "EmployeeCode",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
}
```

### 複合キーフィールド

```json
{
  "DbColumn": "composite_id",
  "Placeholder": "",
  "IsManualInput": false,
  "CompositeIdVariables": ["DepartmentCode.Value", "SequenceNumber.Value"],
  "CompositeIdSeparator": "-",
  "SearchComparisonDefaultValue": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Id",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
}
```

## ランタイム動作

- **UUID自動生成（`IsManualInput = false`）:** レコード新規作成時に `SetTemporaryIdAsync()` が呼ばれ、UUID が自動生成される。ユーザーによる編集は不可。
- **手動入力（`IsManualInput = true`）:** テキスト入力欄が表示され、ユーザーがIDを直接入力する。`Placeholder` でヒントを表示できる。
- **複合キー（`CompositeIdVariables` 指定時）:** `CompositeIdVariables` に列挙したフィールドの値を `CompositeIdSeparator` で結合してIDを構成する。例えば `["DepartmentCode.Value", "SequenceNumber.Value"]` で `CompositeIdSeparator = "-"` の場合、`SALES-001` のようなIDが生成される。
- **更新保護:** `IsUpdateProtected = true` の場合、レコード作成後にIDの変更はできない（主キーの変更防止）。

## 検索

- `SearchComparisonDefaultValue` で検索のデフォルト比較演算子を指定する。
  - `Equal`: 完全一致検索。
  - `Like`: 部分一致検索（パターンマッチ）。
  - `null`: 検索UIでユーザーが選択する。
- `IsSimpleSearchParameter = true` にすると、簡易検索の対象となる。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | string? | 読み書き | IDの値 |
| `SearchValue` | string? | 読み書き | 検索時の値 |
| `SearchComparison` | MatchComparison | 読み書き | 検索時の比較演算子。`Equal`（完全一致）または `Like`（部分一致） |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード（IsManualInput = true）

```html
<input type="text" class="form-control [is-invalid]" style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード（自動採番）

```html
<span class="d-block py-2 text" style="[インラインスタイル]">ID値</span>
```

### CSSセレクタ例

```css
/* ID表示のスタイル */
[data-name="Id"] .text {
  font-family: monospace;
  color: #6c757d;
}
```
