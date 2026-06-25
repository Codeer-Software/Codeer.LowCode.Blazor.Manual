
// 開始日・終了日の前後関係バリデーション
void 開始日_OnDataChanged()
{
    if (開始日.Value != null && 終了日.Value != null && 終了日.Value < 開始日.Value)
    {
        終了日.SetError("終了日は開始日以降を指定してください");
    }
    else
    {
        終了日.ClearError();
    }
}

void 終了日_OnDataChanged()
{
    if (開始日.Value != null && 終了日.Value != null && 終了日.Value < 開始日.Value)
    {
        終了日.SetError("終了日は開始日以降を指定してください");
    }
    else
    {
        終了日.ClearError();
    }
}

// ボード行の詳細ボタン: プロジェクト詳細（メンバー含む）をダイアログ表示
void 詳細ボタン_OnClick()
{
    var dlg = new プロジェクト(ModuleLayoutType.Detail, "ダイアログ用");
    dlg.Id.Value = Id.Value;
    dlg.Reload();
    dlg.ShowDialog("閉じる");
}

// プロジェクト一覧のボードボタン: 該当プロジェクトのカンバンページへ遷移
void ボードボタン_OnClick()
{
    var url = NavigationService.GetModuleDataUrl("プロジェクトボード", Id.Value.ToString());
    NavigationService.NavigateTo(url);
}

// プロジェクト一覧のガントボタン: 該当プロジェクトのガントページへ遷移
void ガントボタン_OnClick()
{
    var url = NavigationService.GetModuleDataUrl("プロジェクトガント", Id.Value.ToString());
    NavigationService.NavigateTo(url);
}