void ShowRightPanel_OnClick()
{
    var p = new ShowPanelTarget(ModuleLayoutType.Detail);
    p.Category.Value = "家電";
    p.MinPrice.Value = 1000;
    p.MaxPrice.Value = 50000;
    if (p.ShowPanel("適用", "キャンセル") == "適用")
    {
        PanelResult.Value = "[右] " + p.Category.Value + " / " + p.MinPrice.Value + "〜" + p.MaxPrice.Value + " 円";
    }
    else
    {
        PanelResult.Value = "[右] キャンセル";
    }
}

void ShowLeftPanel_OnClick()
{
    var p = new ShowPanelTarget(ModuleLayoutType.Detail);
    if (p.ShowPanel(PanelAlignment.Left, "適用", "キャンセル") == "適用")
    {
        PanelResult.Value = "[左] " + p.Category.Value + " / " + p.MinPrice.Value + "〜" + p.MaxPrice.Value + " 円";
    }
    else
    {
        PanelResult.Value = "[左] キャンセル";
    }
}

void OpenPopup_OnClick()
{
    var rect = OpenPopup.GetClientRect();
    int x = (int)rect.Left;
    int y = (int)rect.Bottom + 4;

    var p = new ShowPopupTarget(ModuleLayoutType.Detail);
    if (p.ShowPopup(x, y, "OK", "キャンセル") == "OK")
    {
        PopupResult.Value = p.Memo.Value;
    }
    else
    {
        PopupResult.Value = "キャンセル";
    }
}

void AddCommentButton_OnClick()
{
    var dlg = new EditDialogTarget(ModuleLayoutType.Detail);
    if (dlg.ShowDialog("投稿", "キャンセル") == "投稿")
    {
        Comments.AddRow(dlg);
    }
}
