// 検索用モジュール (クエリ) は読み取り専用で行が閲覧専用になるため、
// 遷移するだけの「開く」ボタンは表示制御を明示的に解除する
void OnAfterInitialization()
{
    OpenRequestButton.IsViewOnly = false;
}

// 「開く」: 申請書 (TargetModuleName/TargetId) へ遷移する
void OpenRequest_OnClick()
{
    if (TargetModuleName.Value == null || TargetId.Value == null) return;
    NavigationService.NavigateTo(NavigationService.GetModuleDataUrl(TargetModuleName.Value, TargetId.Value));
}
