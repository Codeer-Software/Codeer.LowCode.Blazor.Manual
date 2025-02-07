var sercher = new ModuleSearcher<VideoConferencingAccount>();
var accountData = sercher.Execute();

void DetailLayoutDesign_OnAfterInitialization()
{
    if(LoginUserLink.Value == "" || LoginUserLink.Value == null)
    {
        LoginUserLink.Value = "001";
    }
    
}

void LoginUserLink_OnDataChanged()
{
    SetAccount();
}

void SetAccount()
{
    foreach(var i in accountData)
    {
        if(LoginUserLink.Value == i.LoginAccountLink.Value)
        {
            AccountNameText.Value = i.LoginAccountLink.UserName.Value;
            LoginStatusSelect.Value = i.LoginStatusSelect.Value;
            LoginStatusBoolean.Value = i.LoginStatusBoolean.Value;
            ProfileText.Value = i.ProfileText.Value;
            break;
        }
    }
}
void LoginStatusBoolean_OnDataChanged()
{
    if(LoginStatusBoolean.Value == true)
    {
        LoginStatusSelect.IsViewOnly = true;
    }
    else
    {
        LoginStatusSelect.IsViewOnly = false;
    }
}
void FluentButton_OnClick()
{
    Toaster.Error("デモ用のため保存できません");
}