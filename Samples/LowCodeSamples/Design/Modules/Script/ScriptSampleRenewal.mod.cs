void ScriptTest_OnDetailInitialize()
{
    Left.Value = 100;
    Right.Value = 20;
    Operator.Value = "+";
    TargetText.Value = "Change states.";
    TargetDate.Value = DateOnly.FromDateTime(DateTime.Now);
    ColorText.Value = "abc";
    ColorNumber.Value = "123";
    ControlForeground.Value = "#831843";
    ControlBackground.Value = "#FDF2F8";
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
    TargetDate.IsEnabled = !TargetDate.IsEnabled;
    TargetBoolean.IsEnabled = !TargetBoolean.IsEnabled;
    TargetButton.IsEnabled = !TargetButton.IsEnabled;
}

void ButtonVisible_OnClick()
{
    TargetText.IsVisible = !TargetText.IsVisible;
    TargetDate.IsVisible = !TargetDate.IsVisible;
    TargetBoolean.IsVisible = !TargetBoolean.IsVisible;
    TargetButton.IsVisible = !TargetButton.IsVisible;
}

void ButtonViewOnly_OnClick()
{
    TargetText.IsViewOnly = !TargetText.IsViewOnly;
    TargetDate.IsViewOnly = !TargetDate.IsViewOnly;
    TargetBoolean.IsViewOnly = !TargetBoolean.IsViewOnly;
    TargetButton.IsViewOnly = !TargetButton.IsViewOnly;
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

void PopupButton_OnClick()
{
    var dlg = new PersonalInfoDialog();
    if (dlg.ShowPopup(500, 100, "OK", "Cancel") == "OK")
    {
        PopupResult.Value = "Name : " + dlg.Name.Value + "\r\n" + 
                             "Age : " + dlg.Age.Value + "\r\n" + 
                             "Height : " + dlg.Height.Value + "\r\n" + 
                             "Body weight : " + dlg.BodyWeight.Value;
    }
    else
    {
        PopupResult.Value = "";
    }
}

void PanelButton_OnClick()
{
    var dlg = new PersonalInfoDialog();
    if (dlg.ShowPanel("OK", "Cancel") == "OK")
    {
        PanelResult.Value = "Name : " + dlg.Name.Value + "\r\n" + 
                             "Age : " + dlg.Age.Value + "\r\n" + 
                             "Height : " + dlg.Height.Value + "\r\n" + 
                             "Body weight : " + dlg.BodyWeight.Value;
    }
    else
    {
        PanelResult.Value = "";
    }
}

void ColorSetButton_OnClick()
{
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