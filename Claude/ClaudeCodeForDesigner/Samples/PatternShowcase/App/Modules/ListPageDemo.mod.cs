
void OpenEditDialog_OnClick()
{
    var dlg = new ProductEditDialog(ModuleLayoutType.Detail);
    dlg.Id.Value = Id.Value;
    dlg.Reload();
    if (dlg.ShowDialog("保存", "キャンセル") == "保存")
    {
        dlg.Submit();
        Reload();
    }
}
