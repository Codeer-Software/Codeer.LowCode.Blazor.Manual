# ImageViewerField - 画像表示

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ImageViewerFieldDesign`

リソースフォルダの静的画像を表示するフィールド。ロゴやアイコン等の装飾的な画像表示に使用する。
`FieldDesignBase` を直接継承する。DBマッピングなし。

## C# クラス定義 (真実の源)

```csharp
public class ImageViewerFieldDesign : FieldDesignBase
{
    public string ResourcePath { get; set; } = string.Empty;
    public ObjectFit ObjectFit { get; set; } = ObjectFit.Contain;   // enum: Contain/Cover/Fill/None/ScaleDown
    public string OnClick { get; set; } = string.Empty;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ResourcePath` | string | `""` | `Resources/` フォルダ内の画像ファイルパス。 |
| `ObjectFit` | ObjectFit | `"Contain"` | 画像のフィットモード。CSS object-fit に対応。 |
| `OnClick` | string | `""` | クリック時のスクリプトイベント名。設定すると画像がクリック可能になる。 |

## 列挙型

### ObjectFit

| 値 | 説明 |
|---|---|
| `None` | サイズ調整なし |
| `Contain` | アスペクト比を維持して全体を表示 |
| `Cover` | アスペクト比を維持して領域を埋める |
| `Fill` | 領域に合わせて引き伸ばす |
| `ScaleDown` | None と Contain の小さい方 |

## JSON例

### ロゴ画像の表示

```json
{
  "ResourcePath": "lc_logo_256.png",
  "ObjectFit": "Contain",
  "OnClick": "",
  "Name": "Logo",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ImageViewerFieldDesign"
}
```

### クリック可能な画像

```json
{
  "ResourcePath": "banner.png",
  "ObjectFit": "Cover",
  "OnClick": "Banner_OnClick",
  "Name": "Banner",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ImageViewerFieldDesign"
}
```

## ランタイム動作

- `<img>` タグとしてレンダリングされ、`ResourcePath` で指定されたリソース画像を表示する。
- `ObjectFit` で画像のサイズ調整方式を制御する:
  - `Contain`: アスペクト比を維持して全体を表示
  - `Cover`: アスペクト比を維持して領域を埋める
  - `Fill`: 領域に合わせて引き伸ばす
  - `None`: サイズ調整なし
  - `ScaleDown`: None と Contain の小さい方
- `OnClick` が設定されている場合、画像がクリック可能になり、クリック時にスクリプトイベントが実行される。

## 検索

検索には対応しない。DBマッピングなし。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Base64Data` | string | 読み書き | Base64エンコードされた画像データ |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetBase64Data(string fileName, string value)` | void | Base64データから画像を設定する |
| `SetMemoryStream(string fileName, MemoryStream stream)` | void | MemoryStreamから画像を設定する |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 画像表示

```html
<img src="[リソースパス]" style="object-fit: [ObjectFit値]; [インラインスタイル]" />
```

### CSSセレクタ例

```css
/* 画像のスタイル */
[data-name="Logo"] img {
  max-height: 100px;
  border-radius: 0.25rem;
}

/* 画像フィールド全体 */
[data-name="Logo"].field-layout {
  text-align: center;
}
```
