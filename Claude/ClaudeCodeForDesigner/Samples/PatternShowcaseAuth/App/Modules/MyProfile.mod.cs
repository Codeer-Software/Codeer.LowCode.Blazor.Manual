void OnAfterInitialization()
{
    ユーザー識別名.Value = CurrentUser.ユーザー識別名.Value;
    表示名.Value = CurrentUser.表示名.Value;
    ロール.Value = CurrentUser.Role.Value;
}

void ChangePasswordButton_OnClick()
{
    var p = new ChangePasswordDialog(ModuleLayoutType.Detail);
    p.Id.Value = CurrentUser.Id.Value;
    p.Reload();
    if (p.ShowDialog("変更", "キャンセル") == "変更")
    {
        var ret = p.Submit();
        if (ret == true) Toaster.Success("パスワードを変更しました");
        else if (ret == false) Toaster.Error("パスワード変更に失敗しました");
    }
}
