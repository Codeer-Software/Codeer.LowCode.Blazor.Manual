
void ダイアログ登録ボタン_OnClick()
{
    if (this.Submit())
    {
        this.CloseDialog("登録");
    }
    else
    {
        Toaster.Error("入庫登録に失敗しました");
    }
}
