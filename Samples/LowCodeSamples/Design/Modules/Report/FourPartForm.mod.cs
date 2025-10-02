
void DetailLayoutDesign_OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    var today = DateOnly.FromDateTime(DateTime.Now);
    取引先.Value = "デバイス流通株式会社";
    納入先.Value = "本社倉庫";
    注文日.Value = today;
    希望納期.Value = today.AddDays(7);
    注文番号.Value = "PO-1001-000001";
    納品書番号.Value = "DN-000123";
    
    var row = 四連伝票明細リスト.AddRow();
    row.品目コード.Value = "ITM0001";
    row.品目名.Value = "ワイヤレスマウス";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    row.入庫場所.Value = "本社倉庫";
    合計金額.Value += row.合計.Value;
    
    row = 四連伝票明細リスト.AddRow();
    row.品目コード.Value = "ITM0002";
    row.品目名.Value = "メカニカルキーボード";
    row.数量.Value = 3;
    row.単価.Value = 7980;
    row.入庫場所.Value = "本社倉庫";
    合計金額.Value += row.合計.Value;
}

void PDFボタン_OnClick()
{
    発行日.Value = DateOnly.FromDateTime(DateTime.Now).ToString();
    using(var memory = Resources.GetMemoryStream("FourPartForm.xlsx"))
    {
        var excel = new Excel(memory, "FourPartForm");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}