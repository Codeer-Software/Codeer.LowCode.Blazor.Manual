# MarkupStringField - HTMLマークアップ表示

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.MarkupStringFieldDesign`

生のHTMLマークアップをフォーム上に表示するフィールド。リソースファイルからのHTML読み込み、またはインラインでのHTML記述に対応する。`FieldDesignBase` を直接継承する。

## C# クラス定義 (真実の源)

```csharp
public class MarkupStringFieldDesign : FieldDesignBase
{
    public string ResourcePath { get; set; } = string.Empty;
    public string RawHtml { get; set; } = string.Empty;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ResourcePath` | string | `""` | Resources/ フォルダ内のHTMLリソースファイルパス。指定がある場合、こちらが優先される。 |
| `RawHtml` | string | `""` | インラインHTMLマークアップ文字列。`ResourcePath` が空の場合にこちらが使用される。 |

## JSON例

### リソースファイルからHTML表示

```json
{
  "ResourcePath": "notice.html",
  "RawHtml": "",
  "IgnoreModification": false,
  "Name": "NoticeHtml",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.MarkupStringFieldDesign"
}
```

### インラインHTML表示

```json
{
  "ResourcePath": "",
  "RawHtml": "<div class=\"alert alert-info\"><strong>お知らせ:</strong> メンテナンス予定があります。</div>",
  "IgnoreModification": false,
  "Name": "InfoMessage",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.MarkupStringFieldDesign"
}
```

### 装飾テキストの表示

```json
{
  "ResourcePath": "",
  "RawHtml": "<p style=\"color: red; font-weight: bold;\">必須項目を入力してください</p>",
  "IgnoreModification": false,
  "Name": "RequiredNotice",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.MarkupStringFieldDesign"
}
```

## ランタイム動作

- Blazor の `MarkupString` を使用してHTMLをレンダリングする。
- **優先順位:** `ResourcePath` が設定されている場合はリソースファイルからHTMLを読み込む。`ResourcePath` が空の場合は `RawHtml` の値がそのまま使用される。
- リソースファイルは `Resources/` フォルダからの相対パスで指定する。
- HTMLはサニタイズされずにそのままレンダリングされるため、信頼できるHTMLのみを使用すること。
- DB列マッピングなし。検索対象外。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `RawHtml` | string | 読み書き | HTMLマークアップ文字列。スクリプトから動的にHTMLを変更できる。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 表示

```html
<!-- Text プロパティの内容がそのまま HTML として描画される -->
<!-- 例: Text = "<div class='alert alert-info'>お知らせ</div>" の場合 -->
<div class="alert alert-info">お知らせ</div>
```

**注意:** MarkupStringField は外側のラッパー（`field-layout` div）内に、`Text` の HTML がそのまま出力される。CSS でスタイルを適用する場合は `data-name` 属性でフィールドを特定する。

### CSSセレクタ例

```css
/* MarkupString フィールド内のコンテンツ */
[data-name="Notice"] > * {
  margin-bottom: 0;
}
```
