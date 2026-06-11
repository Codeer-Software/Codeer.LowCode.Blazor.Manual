void OpenPopup_OnClick()
{
    var rect = OpenPopup.GetClientRect();
    int x = (int)rect.Left;
    int y = (int)rect.Bottom + 4;

    var p = new ShowPopupTarget(ModuleLayoutType.Detail);
    if (p.ShowPopup(x, y, "OK", "キャンセル") == "OK")
    {
        Result.Value = p.Memo.Value;
    }
    else
    {
        Result.Value = "キャンセル";
    }
}
