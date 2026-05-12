# NumberField - 数値入力フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign`

数値データ（decimal 型）を入力するフィールド。通常の数値入力（`<input type="number">`）とスライダー（`<input type="range">`）を切り替え可能。最小値・最大値のバリデーション、小数桁数の制御、表示フォーマットをサポートする。

## C# クラス定義 (真実の源)

```csharp
public class NumberFieldDesign : DbValueFieldDesignBase
{
    public override string DbColumn { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public bool IsSlider { get; set; }
    public decimal? Step { get; set; }
    public int? MaxFractionDigits { get; set; }
    // 親階層から継承 (詳細は _FieldCommon.md)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `Placeholder` | string | `""` | 入力欄に表示するプレースホルダーテキスト。 |
| `Format` | string | `""` | 表示フォーマット文字列。.NET の数値書式（例: `"N2"` で小数2桁、`"C0"` で通貨形式）。表示専用モードで使用される。 |
| `Min` | decimal? | `null` | 最小値。`ValidateInput()` で検証される。HTML の `min` 属性にも設定される。 |
| `Max` | decimal? | `null` | 最大値。`ValidateInput()` で検証される。HTML の `max` 属性にも設定される。 |
| `IsSlider` | bool | `false` | `true` でスライダー（レンジ入力）、`false` で通常の数値入力。 |
| `Step` | decimal? | `null` | 入力のステップ値。スライダーや数値入力の増減単位。HTML の `step` 属性に設定される。 |
| `MaxFractionDigits` | int? | `null` | 最大小数桁数。`SetValueAsync` 内で指定桁数を超える小数部を切り捨て（truncate）する。丸め（round）ではない点に注意。**整数のみ（`2.0` は不可、`2` と書くこと）** |

## JSON例

### 基本的な数値フィールド（価格）

```json
{
  "DbColumn": "price",
  "Placeholder": "0",
  "Format": "N0",
  "Min": 0,
  "Max": null,
  "IsSlider": false,
  "Step": null,
  "MaxFractionDigits": 0,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "価格",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Price",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

### 小数を含む数値フィールド（数量）

```json
{
  "DbColumn": "quantity",
  "Placeholder": "0.00",
  "Format": "N2",
  "Min": 0.01,
  "Max": 99999.99,
  "IsSlider": false,
  "Step": 0.01,
  "MaxFractionDigits": 2,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "数量",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "Quantity_OnDataChanged",
  "Name": "Quantity",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

### スライダーフィールド（評価）

```json
{
  "DbColumn": "rating",
  "Placeholder": "",
  "Format": "",
  "Min": 1,
  "Max": 5,
  "IsSlider": true,
  "Step": 1,
  "MaxFractionDigits": 0,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "評価",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Rating",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

## ランタイム動作

- **通常入力（`IsSlider = false`）:** `<input type="number">` としてレンダリングされる。`Min`, `Max`, `Step` が HTML 属性として設定される。
- **スライダー（`IsSlider = true`）:** `<input type="range">` としてレンダリングされる。`Min`, `Max`, `Step` でスライダーの範囲と刻みを制御する。
- **Min/Max バリデーション:** `ValidateInput()` で最小値・最大値がチェックされる。違反時はエラーメッセージが表示される。
- **MaxFractionDigits:** `SetValueAsync` 内で値の小数部が指定桁数を超える場合に切り捨てる。例えば `MaxFractionDigits = 2` で `3.14159` を入力すると `3.14` に切り捨てられる。丸め（四捨五入）ではなく切り捨て（truncate）である点に注意。
- **Format:** 表示専用モード（閲覧時）で数値を書式化して表示する。編集モードでは Format は適用されない。
- **表示専用モード:** `Format` に従ってフォーマットされた値が表示される。

## 検索

- 範囲検索（Range Search）をサポートする。検索UIには最小値（SearchMin）と最大値（SearchMax）の2つの入力欄が表示される。
- 検索条件として `GreaterThanOrEqual`（以上）と `LessThanOrEqual`（以下）の条件が生成される。
  - SearchMin のみ指定: `>= SearchMin` の条件。
  - SearchMax のみ指定: `<= SearchMax` の条件。
  - 両方指定: `>= SearchMin AND <= SearchMax` の範囲条件。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | decimal? | 読み書き | 数値の値 |
| `SearchMin` | decimal? | 読み書き | 検索時の最小値 |
| `SearchMax` | decimal? | 読み書き | 検索時の最大値 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード（数値入力）

```html
<input type="number" class="form-control [is-invalid]"
       min="..." max="..." step="..." placeholder="..."
       style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### 編集モード（スライダー: IsSlider = true）

```html
<input type="range" class="form-range"
       min="..." max="..." step="..." />
<span>現在値</span>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">フォーマット済み数値</span>
```

### CSSセレクタ例

```css
/* 数値入力を右寄せ */
[data-name="Price"] .form-control {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

/* スライダーのスタイル */
[data-name="Rating"] .form-range {
  accent-color: #ffc107;
}

/* 表示モードの数値 */
[data-name="Price"] .text {
  font-weight: bold;
  color: #dc3545;
}
```
