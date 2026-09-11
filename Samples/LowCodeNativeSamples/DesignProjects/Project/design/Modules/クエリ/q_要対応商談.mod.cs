void 詳細ボタン_OnClick()
{
    var url = NavigationService.GetModuleDataUrl("商談", Id.Value.ToString());
    NavigationService.NavigateTo(url);
}
