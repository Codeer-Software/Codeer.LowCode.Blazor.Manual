# ColorPickerField - カラーピッカーフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.ColorPickerFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

色を選択して DB 列に保存する値フィールド。HTML5 ネイティブのカラーピッカー（`<input type="color">`）を使用し、選択した色を `#rrggbb` 形式の HEX 文字列として保存する。編集モードではカラースウォッチ（色チップ）と HEX コードを表示し、スウォッチのクリックで OS / ブラウザ標準のカラーピッカーが開く。

## C# クラス定義 (真実の源)

外部リポジトリ `Codeer.LowCode.Blazor.Extras` の `Designs/ColorPickerFieldDesign.cs` で定義されている。`ValueFieldDesignBase` を直接継承する（`DbValueFieldDesignBase` ではない）点に注意。

```csharp
public class ColorPickerFieldDesign() : ValueFieldDesignBase(typeof(ColorPickerFieldDesign).FullName!)
{
    public string DbColumn { get; set; } = string.Empty;   // [DbColumn(nameof(ColorPickerFieldData.Value))]
    public string Default { get; set; } = "#000000";
    // 親階層から継承 (詳細は _FieldCommon.md)
}
```

データクラスは `ValueFieldDataBase<string>`（文字列値）。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput, DisplayName, IsRequired, OnDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 色文字列（`#rrggbb`）を保存する DB 列名。snake_case 推奨。 |
| `Default` | string | `"#000000"` | 値が空のときに**表示する**初期色。フィールド値自体は空のまま保持される（後述）。 |

**注意:** `ValueFieldDesignBase` 直接継承のため、`DbValueFieldDesignBase` のプロパティ（`IsUpdateProtected` / `IsSimpleSearchParameter` / `AllowEmptySearch` / `OnSearchDataChanged`）は**持たない**。

## 必要な DB 構成

`#rrggbb` 形式の文字列（7文字）を格納する。文字列カラムを用意する。

```sql
color_code VARCHAR(16) NULL
```

## JSON例

### デフォルト（Defaults/ColorPickerFieldDesign.json と同一）

```json
{
  "DbColumn": "",
  "Default": "#000000",
  "DisplayName": "",
  "IsRequired": false,
  "OnDataChanged": "",
  "Name": "",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.ColorPickerFieldDesign"
}
```

### 基本的なカラーピッカー（カテゴリ表示色）

```json
{
  "DbColumn": "color_code",
  "Default": "#008FFB",
  "DisplayName": "表示色",
  "IsRequired": false,
  "OnDataChanged": "",
  "Name": "ColorCode",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.ColorPickerFieldDesign"
}
```

## ランタイム動作

- **編集モード:** `<input type="color">`（24px の角丸スウォッチ）と、選択中の HEX コード文字列を横並びで表示する。スウォッチのクリックで OS / ブラウザ標準のカラーピッカーが開き、選択するとその場で値が反映される。
- **表示専用モード（IsViewOnly）:** 色チップ（24px の角丸スウォッチ）と HEX コード文字列のみを表示する。
- **Default の扱い:** `Value` が空のとき、表示上は `Default` の色が表示されるが、**フィールド値自体は空のまま**。Default が表示されているだけでは DB に値は保存されず、`IsRequired = true` なら検証エラーになる。
- **バリデーション:** `IsRequired = true` で `Value` が空または空白のみのとき入力エラーになる。HEX 形式の妥当性チェックは行わない（HTML5 カラーピッカーが常に `#rrggbb` 形式で値を返すため）。
- **IsEnabled = false:** `<input>` に `disabled` が付き、操作不可になる。
- **検索非対応:** 検索用コンポーネントを持たない（`GetSearchWebComponentTypeFullName()` が空）ため、SearchLayout には配置できない。
- **デザインチェック:** フィールド名・`DbColumn` の実在・`OnDataChanged` の関数存在が designcheck で検証される。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | string? | 読み書き | 選択中の色文字列（`#rrggbb`）。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_ScriptApi.md](_ScriptApi.md) を参照。

### イベントハンドラ例

```csharp
// 選択した色をプレビュー用ラベルの背景色に反映する
void ColorCode_OnDataChanged()
{
    PreviewLabel.BackgroundColor = ColorCode.Value;
}
```

---

## DOM構造（CSS用）

### 編集モード

```html
<label class="colorpicker-edit">
  <input class="colorpicker-input" type="color" id="..." />
  <span class="colorpicker-text" style="[インラインスタイル]">#rrggbb</span>
</label>
```

### 表示モード

```html
<div class="colorpicker-view">
  <div class="colorpicker-tip" style="background:#rrggbb"></div>
  <span class="colorpicker-text" style="[インラインスタイル]">#rrggbb</span>
</div>
```

- フィールドの `Color` / `BackgroundColor`（スクリプト/レイアウトのカスケード値）は、CSS 変数 `--colorpicker-foreground` / `--colorpicker-background` として `.colorpicker-text`（HEX コード文字列部分）に適用される。スウォッチ部分の色は常に選択中の色。
- スウォッチは 24px × 24px、角丸 8px。

### CSSセレクタ例

```css
/* HEX コード文字列のフォント */
[data-name="ColorCode"] .colorpicker-text {
  font-family: monospace;
}

/* スウォッチを大きくする（編集モード） */
[data-name="ColorCode"] .colorpicker-input,
[data-name="ColorCode"] .colorpicker-input::-webkit-color-swatch {
  width: 32px;
  height: 32px;
}

/* スウォッチを大きくする（表示モード） */
[data-name="ColorCode"] .colorpicker-tip {
  width: 32px;
  height: 32px;
}
```
