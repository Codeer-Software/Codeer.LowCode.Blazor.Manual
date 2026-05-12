void OnBeforeInitialization()
{
    KPIサマリー.AllowLoad = false;
    要対応サマリー.AllowLoad = false;
    フェーズ別商談本数チャート.AllowLoad = false;
    フェーズ別予測金額チャート.AllowLoad = false;
    営業担当者別受注金額チャート.AllowLoad = false;
    担当者別活動件数チャート.AllowLoad = false;
}

void OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    対象年月.Value = DateTime.Today;
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
    要対応サマリー.AllowLoad = true;
    フェーズ別商談本数チャート.AllowLoad = true;
    フェーズ別予測金額チャート.AllowLoad = true;
    営業担当者別受注金額チャート.AllowLoad = true;
    担当者別活動件数チャート.AllowLoad = true;
    KPIサマリー.Reload();
    要対応サマリー.Reload();
    フェーズ別商談本数チャート.Reload();
    フェーズ別予測金額チャート.Reload();
    営業担当者別受注金額チャート.Reload();
    担当者別活動件数チャート.Reload();
}
