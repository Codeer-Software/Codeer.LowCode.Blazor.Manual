
void List_OnDataChanged()
{
    var total = 0;
    foreach(var e in this.List.Rows)
    {
        total += e.Amount.Value;
    }
    Total.Value = total;
}
void AITextAnalyzer_DataImportCompleted()
{
    List_OnDataChanged();
}
void 請求書サンプルダウンロードボタン_OnClick()
{
    using(var memory = Resources.GetMemoryStream("InvoiceSample.xlsx"))
    {
        var excel = new Excel(memory, "InvoiceSample");
        excel.DownloadPdf();
    }
}