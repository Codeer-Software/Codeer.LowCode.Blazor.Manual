void OnAfterInitialization()
{
    // 起動時に1件目を選択して右ペインに表示
    if (Items.Rows.Count > 0)
    {
        Items.SelectedIndex = 0;
    }
}

void Items_OnSelectedIndexChanged()
{
    using var scope = LoadingService.StartLoading(1000);
    if (Items.SelectedIndex < 0) return;
    var row = (ListPageDemo)Items.Rows[Items.SelectedIndex];
    Editor.ChildModule.Id.Value = row.Id.Value;
    Editor.ChildModule.Reload();
}

void SaveButton_OnClick()
{
    using var scope = LoadingService.StartLoading(1000);
    if (Editor.ChildModule == null) return;
    if (Editor.ChildModule.Id.Value == null) return;
    var ret = Editor.ChildModule.Submit();
    if (ret == false) Toaster.Error("更新失敗");
    // 一覧側を再読込して名前変更等を反映
    Items.Reload();
}
