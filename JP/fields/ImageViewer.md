# ImageViewerField (画像表示)

## これは何か

**画像を表示するフィールド**。入力は受けず、表示専用です。画像のソースはリソースパスでも、プログラム的に設定する Base64 やメモリストリームでも構いません。

## いつ使うか

- 固定画像（ロゴ・説明図）の表示
- 動的に生成する画像の表示（グラフ・QR コードなど）
- ファイルアップロードなしで画像を見せるだけの場面

アップロードが必要なら [File](File.md) の `プレビュー表示` を使います。

---

## デザイナでの設定

<img src="../../Image/designer/fields/imageviewer/ImageViewerSample_properties_panel.png" alt="ImageViewerFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `画像表示` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **ResourcePath** | 画像パス（Resources/） | string | `""` | 画像リソースのパス（`Resources/` フォルダからの相対パス） |
| **ObjectFit** | 画像の表示方法 | enum | `Contain` | 画像の収め方（`None` / `Contain` / `Cover` / `Fill` / `ScaleDown`） |
| **OnClick** | クリックイベント | string | `""` | クリック時のスクリプト |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

> ImageViewerField は値を持たないため、`表示名` / `必須` / `DBカラム` などはありません。

---

## ObjectFit（画像の表示方法）

CSS の `object-fit` と同じ仕様です。

| 値 | 挙動 |
|---|---|
| **None** | 原寸表示（はみ出す場合あり） |
| **Contain** | アスペクト比を保って全体が収まるように縮小 |
| **Cover** | アスペクト比を保って領域を埋める（端が切れる場合あり） |
| **Fill** | 領域に合わせて引き伸ばす（アスペクト比崩れる可能性） |
| **ScaleDown** | 原寸か Contain のどちらか小さい方 |

---

## スクリプトから

### プロパティ・メソッド

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `ResourcePath` | string | 画像リソースパス |
| `ImageExtension` | string | 画像の拡張子 |
| `Base64Data` | string | Base64 エンコードされた画像データ |
| `SetBase64Data(fileName, value)` | void | Base64 データで画像を設定 |
| `SetMemoryStream(fileName, MemoryStream)` | void | MemoryStream で画像を設定 |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 動的に画像を切り替える
Logo.ResourcePath = IsDarkMode.Value ? "logo-dark.png" : "logo-light.png";

// MemoryStream で画像を差し替える
var stream = await GenerateChartAsync();
Chart.SetMemoryStream("chart.png", stream);

// クリックで拡大表示など
void Thumbnail_OnClick()
{
    // 独自のダイアログで拡大
}
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [File](File.md) — アップロードが必要な場合
- [AnchorTag](AnchorTag.md) — 画像付きリンク
