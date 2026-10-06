void DetailLayout_OnAfterInitialization()
{
    // デモデータの最初のタスクから、月表示で全体を見せる
    var s = new ModuleSearcher<ExtrasGanttTask>();
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
    Gantt.ViewMode = GanttViewMode.Month;
    Gantt.ViewStart = ((DateTime)first).Date;
}
