void OnAfterInitialization()
{
    if (Items.Rows.Count > 0) return;

    Customer.Value = "株式会社サンプル";
    IssuedAt.Value = DateOnly.FromDateTime(DateTime.Today);

    AddItem("ノートPC", 2, 128000);
    AddItem("ワイヤレスマウス", 5, 2500);
    AddItem("USB-C ケーブル 1m", 10, 980);
    Recalc();
}

void AddItem(string name, decimal quantity, decimal unitPrice)
{
    var row = Items.AddRow();
    row.ItemName.Value = name;
    row.Quantity.Value = quantity;
    row.UnitPrice.Value = unitPrice;
    row.Subtotal.Value = quantity * unitPrice;
}

void Items_OnDataChanged()
{
    Recalc();
}

void Recalc()
{
    decimal total = 0;
    foreach (var row in Items.Rows)
    {
        total += row.Subtotal.Value;
    }
    Total.Value = total;
}

void ExportExcel_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Report.xlsx"))
    {
        var excel = new Excel(memory, "Report");
        excel.OverWrite(this);
        excel.Download();
    }
}

void ExportPdf_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Report.xlsx"))
    {
        var excel = new Excel(memory, "Report");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}
