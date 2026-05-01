# TextField - テキスト入力フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign`

テキストデータを入力するフィールド。単一行入力（`<input>`）と複数行入力（`<textarea>`）を切り替え可能。最大文字数制限、空値の扱い（空文字列 or null）、編集後のトリムなど、テキスト入力に必要な機能を備える。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `Placeholder` | string | `""` | 入力欄に表示するプレースホルダーテキスト。 |
| `IsMultiline` | bool | `false` | `true` でテキストエリア（複数行）、`false` で単一行入力。 |
| `IsAutoFitRows` | bool | `false` | `true` でテキストエリアの高さを内容に応じて自動調整する。`IsMultiline = true` 時のみ有効。 |
| `Rows` | int? | `null` | テキストエリアの行数。`IsMultiline = true` 時のみ有効。`null` の場合はブラウザのデフォルト行数。**整数のみ（`5.0` は不可、`5` と書くこと）** |
| `MaxLength` | int? | `null` | 最大文字数。HTML の `maxlength` 属性と `ValidateInput()` の両方で検証される。**整数のみ（`100.0` は不可、`100` と書くこと）** |
| `TextEditEmptyType` | TextEditEmptyType | `"StringEmpty"` | 空値の扱い。 |
| `ShouldTrimAfterEdit` | bool | `false` | `true` で編集完了時に前後の空白をトリムする。`SetValueAsync` 内で実行される。 |
| `SearchComparisonDefaultValue` | MatchComparison? | `null` | 検索時のデフォルト比較演算子。`Equal` または `Like` を指定可能。 |

## 列挙型

### TextEditEmptyType

| 値 | 説明 |
|---|---|
| `StringEmpty` | 空文字列として保持 |
| `Null` | null として保持 |

## JSON例

### 基本的なテキストフィールド（名前入力）

```json
{
  "DbColumn": "name",
  "Placeholder": "氏名を入力",
  "IsMultiline": false,
  "IsAutoFitRows": false,
  "Rows": null,
  "MaxLength": 100,
  "TextEditEmptyType": "StringEmpty",
  "ShouldTrimAfterEdit": true,
  "SearchComparisonDefaultValue": "Like",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": true,
  "OnSearchDataChanged": "",
  "DisplayName": "氏名",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Name",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

### 複数行テキストフィールド（備考欄）

```json
{
  "DbColumn": "remarks",
  "Placeholder": "備考を入力してください",
  "IsMultiline": true,
  "IsAutoFitRows": true,
  "Rows": 5,
  "MaxLength": 2000,
  "TextEditEmptyType": "Null",
  "ShouldTrimAfterEdit": false,
  "SearchComparisonDefaultValue": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "備考",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Remarks",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

### メールアドレスフィールド

```json
{
  "DbColumn": "email",
  "Placeholder": "example@example.com",
  "IsMultiline": false,
  "IsAutoFitRows": false,
  "Rows": null,
  "MaxLength": 256,
  "TextEditEmptyType": "Null",
  "ShouldTrimAfterEdit": true,
  "SearchComparisonDefaultValue": "Equal",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": true,
  "OnSearchDataChanged": "",
  "DisplayName": "メールアドレス",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Email",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

## ランタイム動作

- **単一行（`IsMultiline = false`）:** `<input type="text">` としてレンダリングされる。
- **複数行（`IsMultiline = true`）:** `<textarea>` としてレンダリングされる。
  - `Rows` でテキストエリアの初期表示行数を指定する。
  - `IsAutoFitRows = true` の場合、入力内容に応じてテキストエリアの高さが自動拡張される。
- **MaxLength:** HTML の `maxlength` 属性として設定されるため、ブラウザレベルで入力が制限される。加えて `ValidateInput()` でもサーバーサイドで検証される。
- **TextEditEmptyType:** 入力が空の場合の値の扱いを制御する。
  - `StringEmpty`: 空文字列 `""` として保存される。
  - `Null`: `null` として保存される。DB側で NULL 制約がある場合に使い分ける。
- **ShouldTrimAfterEdit:** `SetValueAsync` 内で値の前後の空白文字をトリムする。氏名やコードなど、不要な空白を除去したい場合に有効。
- **表示専用モード:** 編集不可のときはプレーンテキストとして値が表示される。

## 検索

- `SearchComparisonDefaultValue` で検索のデフォルト比較演算子を指定する。
  - `Equal`: 完全一致検索。
  - `Like`: 部分一致検索（LIKE パターンマッチ）。
  - `null`: 検索UIでユーザーが選択する。
- `IsSimpleSearchParameter = true` にすると、簡易検索の対象に含まれる。
- `OnSearchDataChanged` で検索値変更時のスクリプトイベントを指定できる。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | string? | 読み書き | テキストの値 |
| `SearchValue` | string? | 読み書き | 検索時の値 |
| `SearchComparison` | MatchComparison | 読み書き | 検索時の比較演算子。`Equal`（完全一致）または `Like`（部分一致） |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード（単一行）

```html
<input type="text" class="form-control [is-invalid]"
       placeholder="..." maxlength="..." style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### 編集モード（複数行: IsMultiline = true）

```html
<textarea class="form-control [is-invalid]"
          rows="..." placeholder="..." maxlength="..."
          style="[インラインスタイル]"></textarea>
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">テキスト値</span>
```

### CSSセレクタ例

```css
/* テキストフィールド全般 */
[data-name="ProductName"] .form-control {
  font-size: 1.2rem;
}

/* 複数行テキスト */
[data-name="Description"] textarea.form-control {
  min-height: 100px;
}

/* 表示モードのテキスト */
[data-name="ProductName"] .text {
  font-weight: bold;
}
```
