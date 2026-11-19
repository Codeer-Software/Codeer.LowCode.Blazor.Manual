// Job (Jobs/ReorderReport.job.cs) を画面のボタンから動かし、返ってきた結果 (args.Result) を画面に出す。
// 失敗したときは JobTriggerField が通知を出すので、ここでは何もしない
void 実行ボタン_OnClick()
{
    if (!発注候補表Job.Run()) return;
    var r = 発注候補表Job.Result;
    品目数.Value = r.ItemCount;
    仕入先数.Value = r.SupplierCount;
    合計金額.Value = r.Total;

    仕入先別.DeleteAllRows();
    foreach (var s in r.Suppliers)
    {
        var row = 仕入先別.AddRow();
        row.仕入先.Value = s.Name;
        row.品目数.Value = s.Count;
        row.金額.Value = s.Amount;
    }

    // Job が作った Excel を画面に表示して、ダウンロードもする
    帳票.SetTemporaryFile(r.FileGuid, r.FileName, r.FileSize);
    FileService.DownloadTemporaryFile("JobDemo", "帳票", r.FileGuid, r.FileName);
}
