
void DetailLayoutDesign_OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    var today = DateOnly.FromDateTime(DateTime.Now);
    請求先名.Value = "ヴィルトン通信株式会社";
    支払期限.Value = today.AddDays(7);
    支払方法セレクト.Value = "銀行振込";
    
    var row = 請求書明細リスト.AddRow();
    row.品目名.Value = "ワイヤレスマウス";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    合計金額.Value += row.合計.Value;
    
    row = 請求書明細リスト.AddRow();
    row.品目名.Value = "メカニカルキーボード";
    row.数量.Value = 3;
    row.単価.Value = 7980;
    合計金額.Value += row.合計.Value;
    
    row = 請求書明細リスト.AddRow();
    row.品目名.Value = "USB-C ハブ 7ポート";
    row.数量.Value = 2;
    row.単価.Value = 3480;
    合計金額.Value += row.合計.Value;
    
    row = 請求書明細リスト.AddRow();
    row.品目名.Value = "27インチモニター";
    row.数量.Value = 4;
    row.単価.Value = 23800;
    合計金額.Value += row.合計.Value;
    
    row = 請求書明細リスト.AddRow();
    row.品目名.Value = "ノートPCスタンド";
    row.数量.Value = 6;
    row.単価.Value = 2580;
    合計金額.Value += row.合計.Value;
}

void PDFボタン_OnClick()
{
    発行日.Value = DateOnly.FromDateTime(DateTime.Now).ToString();
    using(var memory = Resources.GetMemoryStream("Invoice.xlsx"))
    {
        var excel = new Excel(memory, "Invoice");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}