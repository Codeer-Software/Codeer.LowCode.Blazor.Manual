void DetailLayout_OnAfterInitialization()
{
    // デモデータの最初の予定がある月を表示する
    var s = new ModuleSearcher<ExtrasCalendarData>();
    s.Select(e => e.Start);
    var list = s.Execute();
    DateTime? first = null;
    foreach (var row in list)
    {
        var d = row.Start.Value;
        if (d == null) continue;
        if (first == null || d < first) first = d;
    }
    if (first == null) return;
    Calendar.SelectedDate = (DateTime)first;
}
