void OnBeforeInitialization()
{
    KPIサマリー.AllowLoad = false;
}

void OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    対象年月.Value = GetLatestDataDate();
    UpdateTargetMonthString();
    ReloadAll();
}

void 対象年月_OnDataChanged()
{
    using var suspend = SuspendNotifyStateChanged();
    UpdateTargetMonthString();
    ReloadAll();
}

void UpdateTargetMonthString()
{
    if (対象年月.Value != null)
    {
        対象年月文字列.Value = 対象年月.Value.ToString("yyyy-MM");
    }
}

void ReloadAll()
{
    KPIサマリー.AllowLoad = true;
    KPIサマリー.Reload();
}

// デモデータの「今日」に当たる日 (入庫・出庫の最新日。無ければ今日)
DateOnly GetLatestDataDate()
{
    DateOnly? latest = null;
    var r = new ModuleSearcher<入庫>();
    r.Select(e => e.入庫日);
    var rl = r.Execute();
    foreach (var row in rl)
    {
        var d = row.入庫日.Value;
        if (d != null && (latest == null || d > latest)) latest = d;
    }
    var s = new ModuleSearcher<出庫>();
    s.Select(e => e.出庫日);
    var sl = s.Execute();
    foreach (var row in sl)
    {
        var d = row.出庫日.Value;
        if (d != null && (latest == null || d > latest)) latest = d;
    }
    if (latest == null) return DateOnly.FromDateTime(DateTime.Today);
    return (DateOnly)latest;
}
