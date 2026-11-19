// 在庫を点検して、発注点を下回った商品の「発注候補表」(Excel) を作るバッチ。
// 本番では毎朝スケジューラ (タスクスケジューラ・cron・Azure など) から起動し、購買担当にメールで送る想定。
// デモサイトでは画面 (JobDemo) のボタンから起動し、できた Excel をダウンロードする。
string Run(JobArguments args)
{
    // 1. 発注点を下回った商品 (在庫管理の「要発注品」。不足の大きい順) を読む
    var search = new ModuleSearcher<q_要発注品>();
    var items = search.ExecuteRaw();
    if (items.Count == 0) Fail("発注が必要な商品はありません");

    // 2. 単価は商品マスタから
    var prices = new Dictionary<string, decimal>();
    var productSearch = new ModuleSearcher<商品>();
    foreach (var product in productSearch.ExecuteRaw())
    {
        prices[product.商品コード.Value] = product.単価.Value ?? 0;
    }

    // 3. 仕入先ごとにまとめて、帳票のデータ (明細と仕入先別の小計) を作る
    var supplierNames = new List<string>();
    foreach (var item in items)
    {
        var name = item.仕入先名.Value ?? "(仕入先未設定)";
        if (!supplierNames.Contains(name)) supplierNames.Add(name);
    }

    var report = new JobReorderReport.Data();
    report.作成日時 = new DateTimeFieldData(DateTime.Now);
    report.明細 = new ListFieldData();
    report.仕入先別 = new ListFieldData();
    var suppliers = new JsonObject();
    decimal total = 0;
    foreach (var supplierName in supplierNames)
    {
        decimal itemCount = 0;
        decimal amountSum = 0;
        foreach (var item in items)
        {
            if ((item.仕入先名.Value ?? "(仕入先未設定)") == supplierName)
            {
                var quantity = item.推奨数量.Value ?? 0;
                var price = prices.ContainsKey(item.商品コード.Value) ? prices[item.商品コード.Value] : 0;
                var line = new JobReorderReportLine.Data();
                line.仕入先 = new TextFieldData(supplierName);
                line.倉庫 = new TextFieldData(item.倉庫名.Value);
                line.商品コード = new TextFieldData(item.商品コード.Value);
                line.品名 = new TextFieldData(item.品名.Value);
                line.現在庫 = new NumberFieldData(item.現在庫.Value);
                line.発注点 = new NumberFieldData(item.発注点.Value);
                line.推奨数量 = new NumberFieldData(quantity);
                line.単価 = new NumberFieldData(price);
                line.金額 = new NumberFieldData(quantity * price);
                report.明細.Children.Add(line);
                itemCount += 1;
                amountSum += quantity * price;
            }
        }

        var summary = new JobReorderReportSupplier.Data();
        summary.仕入先 = new TextFieldData(supplierName);
        summary.品目数 = new NumberFieldData(itemCount);
        summary.金額 = new NumberFieldData(amountSum);
        report.仕入先別.Children.Add(summary);

        // 画面に返す仕入先別の小計
        var summaryJson = new JsonObject();
        summaryJson.Name = supplierName;
        summaryJson.Count = itemCount;
        summaryJson.Amount = amountSum;
        suppliers.Add(summaryJson);
        total += amountSum;
    }
    report.品目数 = new NumberFieldData(items.Count);
    report.仕入先数 = new NumberFieldData(supplierNames.Count);
    report.合計金額 = new NumberFieldData(total);

    // 4. Excel のテンプレートに書き込み、起動した人に渡すファイルにする
    //    (本番でメールにするなら excel.ToAttachment("発注候補表.xlsx") を MailService.Send の添付に)
    var result = new JsonObject();
    using (var excel = new Excel(Resources.GetMemoryStream("Job/ReorderReport.xlsx"), "ReorderReport.xlsx"))
    {
        excel.OverWrite(report);
        var file = FileService.SaveTemporary("JobDemo", "帳票", $"発注候補表_{DateTime.Now:yyyyMMdd_HHmm}.xlsx", excel.ToMemoryStream());
        result.FileGuid = file.FileGuid;
        result.FileName = file.FileName;
        result.FileSize = file.FileSize;
    }

    // 5. 画面に結果を返す
    result.ItemCount = items.Count;
    result.SupplierCount = supplierNames.Count;
    result.Total = total;
    result.Suppliers = suppliers;
    args.Result = result;
    Logger.Log($"発注候補表: {items.Count} 品目 / {supplierNames.Count} 社 / {total} 円");
    return $"発注候補表を作りました ({items.Count} 品目・{supplierNames.Count} 社)";
}
