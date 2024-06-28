void DetailLayoutDesign_OnAfterInitialization()
{
    if (LeftList.RowCount == 0) 
    {
        RightModule.IsVisible = false;
        return;
    }
    LeftList.SelectedIndex = 0;
}
void LeftList_OnSelectedIndexChanged()
{
    if (LeftList.RowCount == -1) 
    {
        RightModule.IsVisible = false;
        return;
    }
    
    RightModule.IsVisible = true;
    RightModule.ChildModule.Id.Value = LeftList.Rows[LeftList.SelectedIndex].Id.Value;
    RightModule.ChildModule.Reload();
}
void SubmitButton_OnClick()
{
    if (!RightModule.ChildModule.ValidateInput() || !RightModule.ChildModule.Submit())
    {
        Toaster.Error("失敗");
        return;
    }
    Toaster.Success("更新しました");
    LeftList.UpdateRow(LeftList.SelectedIndex, RightModule.ChildModule);
}