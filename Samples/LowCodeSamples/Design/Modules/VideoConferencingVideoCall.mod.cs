bool existUser = true;

void StartButton_OnClick()
{
    CheckUser();
    SetAlartValue();
}

void TileList_OnDataChanged()
{
    CheckUser();
    DeleteAlartValue();
}

void CheckUser()
{
    existUser = true;
    foreach(var i in UserList.Rows)
    {
        if(i.UserLink.Value == "" || i.UserLink.Value == null)
        {
            existUser = false;
        }
    }
}

void SetAlartValue()
{
    if(UserList.RowCount > 0)
    {
        if(existUser)
        {
            URL.Value = "ユーザーに会議URLを通知しました";
        }
        else
        {
            URL.Value = "ユーザー情報が正しくありません";
        }
    }
    else
    {
        URL.Value = "ユーザーがいません";
    }
}

void DeleteAlartValue()
{
    URL.Value = "";
}