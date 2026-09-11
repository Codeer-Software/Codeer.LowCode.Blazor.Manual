void DetailLayoutDesign_OnBeforeInitialization()
{
    要対応リスト.AllowLoad = false;
}

void DetailLayoutDesign_OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    var today = DateTime.Today;
    var firstOfNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);
    基準日.Value = firstOfNextMonth.AddDays(-1);
    Reload();
}

void 基準日_OnDataChanged()
{
    Reload();
}

void Reload()
{
    要対応リスト.AllowLoad = true;
    要対応リスト.Reload();
}
