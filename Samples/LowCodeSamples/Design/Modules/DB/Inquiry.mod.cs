
void DetailLayoutDesign_OnAfterInitialization()
{
    検索.IsVisible = false;
    集計開始日.Value = "2025-01-01";
    集計終了日.Value = "2025-01-10";
    検索.SearchModule.集計開始日.SearchMin = 集計開始日.Value;
    検索.SearchModule.集計終了日.SearchMin = 集計終了日.Value;
    検索.ExecuteSearch();
}

void 検索ボタン_OnClick()
{
    検索.SearchModule.集計開始日.SearchMin = 集計開始日.Value;
    検索.SearchModule.集計終了日.SearchMin = 集計終了日.Value;
    検索.ExecuteSearch();
}

void クリアボタン_OnClick()
{
    集計開始日.Value = "2025-01-01";
    集計終了日.Value = string.Empty;
    検索.ExecuteClear();
}