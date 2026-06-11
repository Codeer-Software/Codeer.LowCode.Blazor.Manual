void OnSearchInitialization()
{
    // 検索初期化: 担当者を CurrentUser に。
    // ※ LinkField の検索値は SearchValue でセットする (.Value は無視される)。
    Assignee.SearchValue = CurrentUser.Id.Value;
}
