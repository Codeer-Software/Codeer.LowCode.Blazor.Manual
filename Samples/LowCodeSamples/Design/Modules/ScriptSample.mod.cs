void ScriptTest_OnDetailInitialize()
{
    Left.Value = 100;
    Right.Value = 20;
    Operator.Value = "+";
    TargetText.Value = "Change states.";
    ColorText.Value = "abc";
    ColorNumber.Value = "123";
}

void Calculate_OnClick()
{
    switch(Operator.Value)
    {
        case "+":
            Result.Value = Left.Value + Right.Value;
            break;
        case "-":
            Result.Value = Left.Value - Right.Value;
            break;
        case "*":
            Result.Value = Left.Value * Right.Value;
            break;
        case "/":
            Result.Value = Left.Value / Right.Value;
            break;
    }
}

void ButtonEnable_OnClick()
{
    TargetText.IsEnabled = !TargetText.IsEnabled;
}

void ButtonVisible_OnClick()
{
    TargetText.IsVisible = !TargetText.IsVisible;
}

void ButtonViewOnly_OnClick()
{
    TargetText.IsViewOnly = !TargetText.IsViewOnly;
}

void GetWeatherForecast_OnClick()
{
    var data = WebApiService.Get("/testapi").JsonObject;
    WeatherForecastList.DeleteAllRows();
    foreach(var e in data)
    {
        var row = new WeatherForecast();
        row.Date.Value = e.Date;
        row.TemperatureC.Value = e.TemperatureC;
        row.TemperatureF.Value = e.TemperatureF;
        row.Summary.Value = e.Summary;
        WeatherForecastList.AddRow(row);
    }    
}

void QuotaionSample_OnClick()
{
    QuotationTitle.Value = "Party ingredients";
    QuotationClient.Value = "XYZ Company";
    QuotationPersonInCharge.Value = "Tom";
    QuotationDetailList.DeleteAllRows();
    
    var row = QuotationDetailList.AddRow();
    row.Title.Value = "リンゴ";
    row.Detail.Value = "2個入り";
    row.Price.Value = 600;
    row.Discount.Value = 0;
    
    row = QuotationDetailList.AddRow();
    row.Title.Value = "洋ナシ";
    row.Detail.Value = "6個入り";
    row.Price.Value = 880;
    row.Discount.Value = 100;    
    
    row = QuotationDetailList.AddRow();
    row.Title.Value = "バナナ";
    row.Detail.Value = "1パック";
    row.Price.Value = 300;
    row.Discount.Value = 0;   
    
    row = QuotationDetailList.AddRow();
    row.Title.Value = "柿";
    row.Detail.Value = "4個入り";
    row.Price.Value = 480;
    row.Discount.Value = 0;
    
    row = QuotationDetailList.AddRow();
    row.Title.Value = "アボカド";
    row.Detail.Value = "1個入り";
    row.Price.Value = 250;
    row.Discount.Value = 0;
    
}

void ExcelDownload_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Quotation.xlsx"))
    {
        var excel = new Excel(memory, "Quotation");
        excel.OverWrite(this);
        excel.Download();
    }
}

void PdfDownload_OnClick()
{
    using(var memory = Resources.GetMemoryStream("Quotation.xlsx"))
    {
        var excel = new Excel(memory, "Quotation");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}

void QuotationDetail_OnDataChanged()
{    
    var val = 0;
    foreach(var e in QuotationDetailList.Rows)
    {
        val += e.Total.Value;
    }
    QuotationTotal.Value = val;
    QuotationTax.Value = Math.Round(QuotationTotal.Value * 0.1, 0, MidpointRounding.AwayFromZero);
    QuotationTotalInTax.Value = QuotationTotal.Value + QuotationTax.Value;
}

void MessageBoxButton_OnClick()
{
    MessageBoxResult.Value = MessageBox.Show("A message box. Select a button and press it.", "OK", "Cancel");
}

void DialogButton_OnClick()
{
    var dlg = new PersonalInfoDialog();
    if (dlg.ShowDialog("OK", "Cancel") == "OK")
    {
        DialogResult.Value = "Name : " + dlg.Name.Value + "\r\n" + 
                             "Age : " + dlg.Age.Value + "\r\n" + 
                             "Height : " + dlg.Height.Value + "\r\n" + 
                             "Body weight : " + dlg.BodyWeight.Value;
    }
    else
    {
        DialogResult.Value = "";
    }
}

void Button_OnClick()
{
    
    ColorBoolean.BackgroundColor = ControlBackground.Value;
    ColorBoolean.Color = ControlForeground.Value;
    
    ColorButton.BackgroundColor = ControlBackground.Value;
    ColorButton.Color = ControlForeground.Value;
    
    ColorDate.BackgroundColor = ControlBackground.Value;
    ColorDate.Color = ControlForeground.Value;
    
    ColorDateTime.BackgroundColor = ControlBackground.Value;
    ColorDateTime.Color = ControlForeground.Value;
    
    ColorNumber.BackgroundColor = ControlBackground.Value;
    ColorNumber.Color = ControlForeground.Value;
    
    ColorText.BackgroundColor = ControlBackground.Value;
    ColorText.Color = ControlForeground.Value;
}