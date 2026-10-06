void DetailLayoutDesign_OnBeforeInitialization()
{
    要対応リスト.AllowLoad = false;
}

void DetailLayoutDesign_OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    var today = GetLatestActivityDate();
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

// デモデータの最新の活動がある日 (無ければ今日)
DateTime GetLatestActivityDate()
{
    var s = new ModuleSearcher<活動履歴>();
    s.Select(e => e.活動日時);
    var list = s.Execute();
    DateTime? latest = null;
    foreach (var row in list)
    {
        var d = row.活動日時.Value;
        if (d == null) continue;
        if (latest == null || d > latest) latest = d;
    }
    if (latest == null) return DateTime.Today;
    return ((DateTime)latest).Date;
}
