# LabelField - ラベル（表示専用）

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign`

表示専用のラベルフィールド。静的テキストの表示、または他フィールドの値をラベルとして表示する。
`FieldDesignBase` を直接継承する。DBマッピングなし。

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | string | `"Label"` | 表示テキスト。複数行に対応。 |
| `Icon` | string | `""` | アイコン識別子。 |
| `Style` | LabelStyle | `"Default"` | テキストスタイル。 |
| `RelativeField` | string | `""` | 設定すると、指定フィールドの値を表示テキストとして使用する。HTMLの `label[for]` 属性も設定されアクセシビリティに対応する。 |
| `OnClick` | string | `""` | クリック時のスクリプトイベント名。設定するとラベルがクリック可能になる。 |

## 列挙型

### LabelStyle

| 値 | 説明 |
|---|---|
| `Default` | 標準テキスト |
| `H1` | 見出し1 |
| `H2` | 見出し2 |
| `H3` | 見出し3 |
| `H4` | 見出し4 |
| `H5` | 見出し5 |
| `H6` | 見出し6 |

## JSON例

### 静的テキストの見出し

```json
{
  "Text": "基本情報",
  "Icon": "",
  "Style": "H2",
  "RelativeField": "",
  "OnClick": "",
  "Name": "SectionHeader",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```

### フィールドのラベルとして使用

```json
{
  "Text": "名前",
  "Icon": "",
  "Style": "Default",
  "RelativeField": "UserName",
  "OnClick": "",
  "Name": "UserNameLabel",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```

### クリック可能なラベル

```json
{
  "Text": "詳細を表示",
  "Icon": "",
  "Style": "Default",
  "RelativeField": "",
  "OnClick": "DetailLabel_OnClick",
  "Name": "DetailLabel",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Text` | string | 表示テキスト（読み書き可能） |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ: OnClick

`OnClick` プロパティに指定したメソッド名が、ラベルクリック時に呼ばれる。

```csharp
void DetailLabel_OnClick()
{
    // ラベルクリック時の処理
    NavigationService.NavigateTo("/details");
}
```

### スクリプトからのテキスト変更例

```csharp
// 動的にラベルテキストを変更
void UpdateStatus_OnDataChanged()
{
    if (Status.Value == "Active")
    {
        StatusLabel.Text = "有効";
        StatusLabel.Color = "#28a745";
    }
    else
    {
        StatusLabel.Text = "無効";
        StatusLabel.Color = "#dc3545";
    }
}
```

## ランタイム動作

- `RelativeField` が指定されている場合、そのフィールドの現在の値をラベルテキストとして表示する。`Text` の値は使用されない。また、HTMLの `<label for="...">` 属性が設定され、ラベルクリックで対象フィールドにフォーカスが移る。
- `RelativeField` が空の場合、`Text` に設定された静的テキストを表示する。
- `Style` は HTML の見出し要素にマッピングされる（`H1` → `<h1>`、`H2` → `<h2>` など）。`Default` は通常のテキスト表示。
- `OnClick` が設定されている場合、ラベルがクリック可能になり、クリック時にスクリプトイベントが実行される。

## 検索

検索には対応しない。DBマッピングなし。

---

## DOM構造（CSS用）

### Default スタイル

```html
<label class="m-0 form-label" style="[インラインスタイル]">
  <span class="[アイコンクラス] me-2" aria-hidden="true"></span>
  テキスト
</label>
```

### H1〜H6 スタイル

```html
<h1 class="m-0" style="[インラインスタイル]">
  <span class="[アイコンクラス] me-2" aria-hidden="true"></span>
  テキスト
</h1>
<!-- H2〜H6 も同様 -->
```

### CSSセレクタ例

```css
/* ラベルのスタイル */
[data-name="TitleLabel"] .form-label {
  color: #495057;
  font-weight: 600;
}

/* 見出しスタイル */
[data-name="SectionTitle"] h2 {
  border-bottom: 2px solid #0d6efd;
  padding-bottom: 0.5rem;
}
```
