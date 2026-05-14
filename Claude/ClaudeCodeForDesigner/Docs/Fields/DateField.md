# DateField - 日付入力フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign`

日付データ（DateOnly 型）を入力するフィールド。日付ピッカー（`<input type="date">`）または年月ピッカー（`<input type="month">`）として動作する。表示専用モードではフォーマット文字列で書式化して表示できる。

## C# クラス定義 (真実の源)

```csharp
public class DateFieldDesign : DbValueFieldDesignBase
{
    public override string DbColumn { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool IsYearMonthOnly { get; set; }
    // 親階層から継承 (詳細は _FieldCommon.md)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `Format` | string | `""` | 表示専用モードでの日付表示フォーマット。.NET の日付書式文字列（例: `"yyyy/MM/dd"`, `"yyyy年M月d日"`）。空の場合はデフォルト形式。 |
| `IsYearMonthOnly` | bool | `false` | `true` で年月ピッカー（yyyy-MM 形式）、`false` で日付ピッカー（yyyy-MM-dd 形式）。 |

## JSON例

### 基本的な日付フィールド（生年月日）

```json
{
  "DbColumn": "birth_date",
  "Format": "yyyy/MM/dd",
  "IsYearMonthOnly": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "生年月日",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "BirthDate",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
}
```

### 年月のみ入力（有効期限月）

```json
{
  "DbColumn": "expiry_month",
  "Format": "yyyy年MM月",
  "IsYearMonthOnly": true,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "有効期限",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "ExpiryMonth",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
}
```

### 登録日フィールド（更新保護あり）

```json
{
  "DbColumn": "created_date",
  "Format": "yyyy/MM/dd",
  "IsYearMonthOnly": false,
  "IsUpdateProtected": true,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "登録日",
  "IsRequired": false,
  "IgnoreModification": true,
  "OnDataChanged": "",
  "Name": "CreatedDate",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
}
```

## ランタイム動作

- **日付ピッカー（`IsYearMonthOnly = false`）:** `<input type="date">` としてレンダリングされる。ブラウザネイティブの日付ピッカーが表示される。値は `yyyy-MM-dd` 形式で保持される。
- **年月ピッカー（`IsYearMonthOnly = true`）:** `<input type="month">` としてレンダリングされる。年と月のみを選択でき、日は保持されない（`yyyy-MM` 形式）。
- **Format:** 表示専用モード（閲覧時）で日付をフォーマットして表示する。
  - 例: `"yyyy/MM/dd"` -> `2025/01/15`
  - 例: `"yyyy年M月d日"` -> `2025年1月15日`
  - 編集モードではブラウザの日付ピッカーが使用されるため、Format は適用されない。

## 検索

- 範囲検索（Range Search）をサポートする。検索UIには最小日付（SearchMin）と最大日付（SearchMax）の2つの日付ピッカーが表示される。
- 検索条件として `GreaterThanOrEqual`（以上）と `LessThanOrEqual`（以下）の条件が生成される。
  - SearchMin のみ指定: `>= SearchMin` の条件。
  - SearchMax のみ指定: `<= SearchMax` の条件。
  - 両方指定: `>= SearchMin AND <= SearchMax` の範囲条件。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | DateOnly? | 読み書き | 日付の値 |
| `SearchMin` | DateOnly? | 読み書き | 検索時の最小日付 |
| `SearchMax` | DateOnly? | 読み書き | 検索時の最大日付 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード

```html
<!-- 通常の日付入力 -->
<input type="date" class="form-control [is-invalid]" style="[インラインスタイル]" />

<!-- 年月のみ（IsYearMonthOnly = true） -->
<input type="month" class="form-control [is-invalid]" style="[インラインスタイル]" />

<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">フォーマット済み日付</span>
```

### CSSセレクタ例

```css
/* 日付入力の幅 */
[data-name="StartDate"] .form-control {
  max-width: 200px;
}
```
