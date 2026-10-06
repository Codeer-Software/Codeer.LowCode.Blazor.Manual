void OnBeforeInitialization()
{
    プロジェクト別遅延件数チャート.AllowLoad = false;
    今月タスク消化率チャート.AllowLoad = false;
    部署別未完了タスク数チャート.AllowLoad = false;
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
    プロジェクト別遅延件数チャート.AllowLoad = true;
    今月タスク消化率チャート.AllowLoad = true;
    部署別未完了タスク数チャート.AllowLoad = true;
    プロジェクト別遅延件数チャート.Reload();
    今月タスク消化率チャート.Reload();
    部署別未完了タスク数チャート.Reload();
}

// デモデータの「今日」に当たる日 (完了したタスクの最新の終了日。無ければ今日)
DateOnly GetLatestDataDate()
{
    var s = new ModuleSearcher<タスク>();
    s.AddEquals(e => e.ステータス.Value, "完了");
    s.Select(e => e.終了日);
    var list = s.Execute();
    DateOnly? latest = null;
    foreach (var row in list)
    {
        var d = row.終了日.Value;
        if (d == null) continue;
        if (latest == null || d > latest) latest = d;
    }
    if (latest == null) return DateOnly.FromDateTime(DateTime.Today);
    return (DateOnly)latest;
}
