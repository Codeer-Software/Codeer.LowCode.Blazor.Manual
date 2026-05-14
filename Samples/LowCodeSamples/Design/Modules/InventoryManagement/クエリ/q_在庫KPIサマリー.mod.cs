
void 要発注品遷移_OnClick()
{
    var url = NavigationService.GetModuleUrl("要発注品");
    NavigationService.NavigateTo(url);
}

void 遅延発注遷移_OnClick()
{
    var url = NavigationService.GetModuleUrl("発注") + "?initialize_search=true&delayed=true";
    NavigationService.NavigateTo(url);
}
