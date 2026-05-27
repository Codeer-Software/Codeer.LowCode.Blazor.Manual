void ShowRightPanel_OnClick()
{
    var p = new ShowPanelTarget(ModuleLayoutType.Detail);
    p.Category.Value = "家電";
    p.MinPrice.Value = 1000;
    p.MaxPrice.Value = 50000;
    if (p.ShowPanel("適用", "キャンセル") == "適用")
    {
        Result.Value = "[右] " + p.Category.Value + " / " + p.MinPrice.Value + "〜" + p.MaxPrice.Value + " 円";
    }
    else
    {
        Result.Value = "[右] キャンセル";
    }
}

void ShowLeftPanel_OnClick()
{
    var p = new ShowPanelTarget(ModuleLayoutType.Detail);
    if (p.ShowPanel(PanelAlignment.Left, "適用", "キャンセル") == "適用")
    {
        Result.Value = "[左] " + p.Category.Value + " / " + p.MinPrice.Value + "〜" + p.MaxPrice.Value + " 円";
    }
    else
    {
        Result.Value = "[左] キャンセル";
    }
}
