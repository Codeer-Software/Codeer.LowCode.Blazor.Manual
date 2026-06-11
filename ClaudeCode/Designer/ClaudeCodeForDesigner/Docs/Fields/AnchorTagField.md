# AnchorTagField - ナビゲーションリンクフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign`

ナビゲーションリンク。URL、別モジュール、ページフレーム、ブラウザ履歴への遷移を実現する。テキストリンクまたはボタン風の表示スタイルを選択可能。

## C# クラス定義 (真実の源)

```csharp
public class AnchorTagFieldDesign : FieldDesignBase
{
    public AnchorStyle Style { get; set; }                // enum: Text / Button / Image
    public AnchorTarget Target { get; set; }              // enum: Url / HistoryBack / HistoryForward
    public bool ShouldOpenInNewTab { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string TitleText { get; set; } = "Anchor Tag";
    public string TitleVariable { get; set; } = string.Empty;
    public string ImageResourcePath { get; set; } = string.Empty;
    public string PageFrame { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ModuleVariable { get; set; } = string.Empty;
    public string IdVariable { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OnClick { get; set; } = string.Empty;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `TitleText` | string | `"Anchor Tag"` | リンクの表示テキスト。 |
| `TitleVariable` | string | `""` | 動的タイトルのための変数パス（例: `Name.Value`）。指定すると `TitleText` を上書きする。 |
| `Style` | AnchorStyle | `"Text"` | 表示スタイル。`Text`（テキストリンク）または `Button`（ボタン風表示）。 |
| `Target` | AnchorTarget | `"Url"` | リンク先の種別。`Url`（外部/内部URL）、`HistoryBack`（ブラウザ戻る）、`HistoryForward`（ブラウザ進む）。 |
| `Url` | string | `""` | リンク先URL。`Target = Url` かつ `Module` が未指定の場合に使用。 |
| `Module` | string | `""` | 遷移先のモジュール名。内部ナビゲーション用。 |
| `ModuleVariable` | string | `""` | 動的モジュール名のための変数パス。指定すると `Module` を上書きする。 |
| `PageFrame` | string | `""` | ナビゲーションのコンテキストとなるページフレーム名。 |
| `IdVariable` | string | `""` | 遷移先レコードIDのための変数パス（例: `Id.Value`）。詳細ページへの遷移に使用。 |
| `ShouldOpenInNewTab` | bool | `false` | `true` でリンクを新しいブラウザタブで開く。 |
| `Icon` | string | `""` | アイコン識別子。 |
| `ImageResourcePath` | string | `""` | リンクに表示する画像リソースのパス。 |
| `OnClick` | string | `""` | クリック時に実行されるスクリプトイベント名。ナビゲーションの前に実行される。 |

## 列挙型

### AnchorStyle

| 値 | 説明 |
|---|---|
| `Text` | テキストリンク |
| `Button` | ボタン風表示 |

### AnchorTarget

| 値 | 説明 |
|---|---|
| `Url` | URL遷移（外部URL / モジュール遷移） |
| `HistoryBack` | ブラウザ履歴を戻る |
| `HistoryForward` | ブラウザ履歴を進む |

## JSON例

### 外部URLへのリンク

```json
{
  "Style": "Text",
  "Target": "Url",
  "ShouldOpenInNewTab": true,
  "Icon": "",
  "TitleText": "公式サイト",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "",
  "Module": "",
  "ModuleVariable": "",
  "IdVariable": "",
  "Url": "https://www.example.com",
  "OnClick": "",
  "IgnoreModification": false,
  "Name": "OfficialSiteLink",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```

### モジュール詳細ページへの遷移リンク

```json
{
  "Style": "Text",
  "Target": "Url",
  "ShouldOpenInNewTab": false,
  "Icon": "",
  "TitleText": "詳細を見る",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "Main",
  "Module": "Product",
  "ModuleVariable": "",
  "IdVariable": "Id.Value",
  "Url": "",
  "OnClick": "",
  "IgnoreModification": false,
  "Name": "DetailLink",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```

### ブラウザ戻るボタン

```json
{
  "Style": "Button",
  "Target": "HistoryBack",
  "ShouldOpenInNewTab": false,
  "Icon": "ArrowLeft",
  "TitleText": "戻る",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "",
  "Module": "",
  "ModuleVariable": "",
  "IdVariable": "",
  "Url": "",
  "OnClick": "",
  "IgnoreModification": false,
  "Name": "BackButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```

### 動的タイトルのリンク（フィールド値を表示テキストに使用）

```json
{
  "Style": "Text",
  "Target": "Url",
  "ShouldOpenInNewTab": false,
  "Icon": "",
  "TitleText": "",
  "TitleVariable": "ProductName.Value",
  "ImageResourcePath": "",
  "PageFrame": "Main",
  "Module": "Product",
  "ModuleVariable": "",
  "IdVariable": "ProductId.Value",
  "Url": "",
  "OnClick": "",
  "IgnoreModification": false,
  "Name": "ProductLink",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```

## ランタイム動作

- `<a>` タグとしてレンダリングされる。`Style = Button` の場合はボタン風の外観が適用される。
- **Module指定時:** 内部URLを自動生成し、指定モジュールのページへ遷移する。`PageFrame` が指定されていれば、そのページフレーム内で遷移する。
- **IdVariable指定時:** 変数パスからレコードIDを取得し、URLにIDを付加する。詳細ページへの遷移に使用。
- **Target = HistoryBack:** `window.history.back()` 相当のブラウザ履歴の戻る操作を実行する。
- **Target = HistoryForward:** `window.history.forward()` 相当のブラウザ履歴の進む操作を実行する。
- **TitleVariable指定時:** 変数パスから動的にタイトルテキストを取得する。`TitleText` より優先される。
- **OnClick指定時:** ナビゲーションの前にスクリプトイベントが実行される。
- データの永続化は行わない。`CreateData()` は `null` を返す。

## 検索

検索には対応しない。DB列マッピングなし。

## スクリプトAPI

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetUrl(string url)` | void | リンク先URLを設定する |
| `GetUrl()` | string | 現在のURLを取得する |
| `SetTitle(string title)` | void | 表示テキストを設定する |
| `GetTitle()` | string | 現在の表示テキストを取得する |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### テキストリンク（Style = "Text"）

```html
<a href="[URL]" target="[_blank]" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  タイトルテキスト
</a>
```

### ボタンリンク（Style = "Button"）

```html
<a class="btn btn-primary" href="[URL]" target="[_blank]" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  タイトルテキスト
</a>
```

### 画像リンク（ImageResourcePath 設定時）

```html
<a href="[URL]" target="[_blank]" style="[インラインスタイル]">
  <img src="[リソースパス]" />
</a>
```

### CSSセレクタ例

```css
/* テキストリンクのスタイル */
[data-name="DetailLink"] a {
  text-decoration: none;
  color: #0d6efd;
}

[data-name="DetailLink"] a:hover {
  text-decoration: underline;
}

/* ボタンリンクのスタイル */
[data-name="ActionLink"] .btn {
  min-width: 120px;
}
```
