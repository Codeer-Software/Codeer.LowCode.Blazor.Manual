# CopyModuleButtonField - レコードコピーボタン

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.CopyModuleButtonFieldDesign`

現在のレコードデータをコピーして新規レコードとして開くボタンフィールド。既存データをテンプレートとして新しいレコードを作成する場合に使用する。`FieldDesignBase` を直接継承する。

## C# クラス定義 (真実の源)

```csharp
public class CopyModuleButtonFieldDesign : FieldDesignBase
{
    public string Text { get; set; } = "Copy";
    [Obsolete] public string ImageResourcePath { get; set; } = string.Empty;
    public ButtonImageSet ImageResourceSet { get; set; } = new();
    public string Icon { get; set; } = string.Empty;
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    public bool IsBlock { get; set; } = true;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | string | `"Copy"` | ボタンに表示するラベルテキスト。 |
| `Icon` | string | `""` | ボタンに表示するアイコン識別子。 |
| `Variant` | ButtonVariant | `"Primary"` | ボタンのスタイル。 |
| `IsBlock` | bool | `true` | `true` でボタンを全幅表示する。 |
| `ImageResourceSet` | ButtonImageSet | `new()` | ボタンの各状態に対応する画像リソース。 |

### ButtonImageSet の構造

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Default` | string | `""` | 通常時の画像リソースパス。 |
| `Focus` | string | `""` | フォーカス時の画像リソースパス。 |
| `Active` | string | `""` | アクティブ（押下）時の画像リソースパス。 |
| `Hover` | string | `""` | ホバー時の画像リソースパス。 |
| `Disabled` | string | `""` | 無効時の画像リソースパス。 |

## 列挙型

### ButtonVariant

| 値 | 説明 |
|---|---|
| `Primary` | 主要アクション（青） |
| `Secondary` | 副次アクション（灰） |
| `Success` | 成功（緑） |
| `Danger` | 危険/削除（赤） |
| `Warning` | 警告（黄） |
| `Info` | 情報（水色） |
| `Light` | 明るい背景 |
| `Dark` | 暗い背景 |
| `Link` | リンク風表示 |
| `Text` | テキストのみ |
| `OutlinePrimary` | 枠線のみ（青） |
| `OutlineSecondary` | 枠線のみ（灰） |
| `OutlineSuccess` | 枠線のみ（緑） |
| `OutlineDanger` | 枠線のみ（赤） |
| `OutlineWarning` | 枠線のみ（黄） |
| `OutlineInfo` | 枠線のみ（水色） |
| `OutlineLight` | 枠線のみ（明） |
| `OutlineDark` | 枠線のみ（暗） |

## JSON例

### 基本的なコピーボタン

```json
{
  "Text": "コピーして新規作成",
  "Icon": "",
  "Variant": "Primary",
  "IsBlock": true,
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "IgnoreModification": false,
  "Name": "CopyButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.CopyModuleButtonFieldDesign"
}
```

### アイコン付きのコンパクトなコピーボタン

```json
{
  "Text": "複製",
  "Icon": "ContentCopy",
  "Variant": "Secondary",
  "IsBlock": false,
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "IgnoreModification": false,
  "Name": "DuplicateButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.CopyModuleButtonFieldDesign"
}
```

## ランタイム動作

- ボタンをクリックすると、現在表示中のモジュールデータがコピーされる。
- コピーされたデータには新しい一時IDが割り当てられ、新規レコードとして編集画面が開かれる。
- 元のレコードは変更されない。
- `Variant` により Bootstrap ベースのボタンスタイルが適用される。
- `IsBlock = true` の場合、ボタンは親コンテナの全幅に広がる。
- DB列マッピングなし。検索対象外。

---

## DOM構造（CSS用）

### ボタン表示

```html
<button class="btn btn-[Variant]" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  テキスト
</button>
```

### CSSセレクタ例

```css
/* コピーボタンのスタイル */
[data-name="CopyButton"] .btn {
  min-width: 100px;
}
```
