# チュートリアル: Excel 帳票と PDF 出力

**所要時間: 約 30 分**

業務アプリでよく求められる Excel・PDF 帳票の出力を行います。
Codeer.LowCode.Blazor は **Excel テンプレートをそのまま使う**方式で、デザインを Excel で作り、スクリプトから値を流し込みます。

---

## 前提

- [はじめてのモジュール作成](../quickstart/first_module.md) を完了
- 見積書や請求書など、Excel 形式の帳票を出したいケースを想定
- Excel がインストールされている必要はありません（サーバー側で生成）

---

## 全体の流れ

```
Excel テンプレート（.xlsx）をリソースとして配置
   ↓
スクリプトで Excel オブジェクトを開く
   ↓
セルに値を書き込む
   ↓
Excel のまま Download または PDF に変換して Download
```

---

## Step 1. Excel テンプレートを用意する

見積書のテンプレートを例にします:

```
┌──────────────────────┐
│ 見積書                │
│ 見積番号: {{Id}}      │
│ 発行日:   {{Date}}    │
│ お客様:   {{Customer}}│
├──────────────────────┤
│ 品名 │ 数量 │ 単価    │
├──────────────────────┤
│      │      │         │
│      │      │         │
└──────────────────────┘
```

固定部分は Excel にそのまま書き込み、値を埋めたいセルには目印となる**検索キーワード**（例: `{{Id}}`）を入れておきます。

テンプレートは Visual Studio プロジェクトの `Resource` フォルダに `.xlsx` として保存します。

---

## Step 2. ボタンに出力処理を書く

### Excel ダウンロード

```csharp
void ExportExcelButton_OnClick()
{
    // テンプレートを Resource から取得
    var templateStream = await Resources.GetMemoryStream("見積書テンプレート.xlsx");

    // Excel オブジェクトを開く
    using var excel = new Excel(templateStream);

    // セルにキーワードから値を書き込む
    var idCell = excel.FindCellByText("{{Id}}");
    if (idCell != null)
    {
        excel.SetCellValue(idCell, Id.Value);
    }

    excel.SetCellValue(excel.FindCellByText("{{Date}}"), OrderDate.Value?.ToString("yyyy/MM/dd"));
    excel.SetCellValue(excel.FindCellByText("{{Customer}}"), Customer.DisplayText);

    // ダウンロード
    await excel.Download("見積書.xlsx");
}
```

### PDF 変換してダウンロード

Excel を開いて値を埋めたあと、PDF に変換して出力します。

```csharp
void ExportPdfButton_OnClick()
{
    var templateStream = await Resources.GetMemoryStream("見積書テンプレート.xlsx");
    using var excel = new Excel(templateStream);

    excel.SetCellValue(excel.FindCellByText("{{Id}}"), Id.Value);
    excel.SetCellValue(excel.FindCellByText("{{Date}}"), OrderDate.Value?.ToString("yyyy/MM/dd"));

    // PDF でダウンロード
    await excel.DownloadPdf("見積書.pdf");
}
```

---

## Step 3. 明細行（可変行）を埋める

リストのデータをテンプレートの表に流し込むには、**セルをコピー**しながら行を増やします。

```csharp
void ExportExcelButton_OnClick()
{
    var templateStream = await Resources.GetMemoryStream("見積書テンプレート.xlsx");
    using var excel = new Excel(templateStream);

    // 明細行の先頭セル（品名列）を探す
    var startCell = excel.FindCellByText("{{ItemName}}");
    if (startCell == null) return;

    // DetailList の各行をセルに流し込む
    for (int i = 0; i < OrderItems.Rows.Count; i++)
    {
        var row = OrderItems.Rows[i];

        // 必要に応じてテンプレート行をコピー
        if (i > 0)
        {
            excel.CopyCells(startCell.RowIndex, startCell.RowIndex + i);
        }

        excel.SetCellValue(startCell.Offset(i, 0), row.ItemName.Value);
        excel.SetCellValue(startCell.Offset(i, 1), row.Quantity.Value);
        excel.SetCellValue(startCell.Offset(i, 2), row.UnitPrice.Value);
    }

    await excel.Download("見積書.xlsx");
}
```

> 正確な API（`CopyCells` の引数、`Offset` の仕様）は環境によって異なる場合があります。実プロジェクトのサンプルを参照してください。

---

## よく使う Excel スクリプト API

| API | 用途 |
|---|---|
| `new Excel(stream)` | テンプレートを開く |
| `FindCellByText(keyword)` | キーワードで検索してセル位置を特定 |
| `SetCellValue(cell, value)` | セルに値を設定 |
| `CopyCells(from, to)` | セル範囲をコピー |
| `Overwrite()` | Excel 上の変更を保存 |
| `Download(fileName)` | Excel をそのままダウンロード |
| `DownloadPdf(fileName)` | PDF に変換してダウンロード |

---

## Tips

### Q. フォント・罫線などの書式も保ちたい

Excel テンプレートの**書式**は `CopyCells` でコピーすれば保たれます。セル単位で設定したい場合は Excel 側で事前にスタイルを作っておくのが最も楽です。

### Q. グラフや画像を含むテンプレートでも大丈夫？

基本的に維持されます。ただし PDF 変換時にフォントや画像の埋め込み状態に依存する挙動があるため、本番環境で必ず確認してください。

### Q. サーバー側で出力したい

ここで説明したのはクライアント側スクリプトの例です。サーバー側で出力する場合は、プロコードで API を作り、スクリプトから WebAPI 経由で呼び出す構成になります。詳しくは [チュートリアル: WebAPI 連携](tutorial_webapi.md)。

---

## 次に読む

- [チュートリアル: WebAPI 連携](tutorial_webapi.md)
- [スクリプト概要](../overview/script.md)
- [プロコード概要](../overview/procode.md)
