void MsgDelete_OnClick()
{
    var answer = MessageBox.Show(
        "このレコードを削除します。元に戻せません。",
        new DangerButton("削除"),
        new SecondaryOutlineButton("キャンセル"));
    Result.Value = "Danger: " + answer;
}

void MsgWarning_OnClick()
{
    var answer = MessageBox.Show(
        "残りディスク容量が少なくなっています。",
        new WarningButton("確認しました"));
    Result.Value = "Warning: " + answer;
}

void MsgSuccess_OnClick()
{
    var answer = MessageBox.ShowWithTitle("送信完了",
        "メッセージを送信しました。",
        new SuccessButton("OK"),
        new InfoOutlineButton("送信履歴"));
    Result.Value = "Success/Info: " + answer;
}

void MsgPrimaryOutline_OnClick()
{
    var answer = MessageBox.Show(
        "この操作を確定しますか？",
        new PrimaryButton("確定", true),
        new SecondaryOutlineButton("キャンセル"));
    Result.Value = "Primary+Outline: " + answer;
}

void DlgDelete_OnClick()
{
    var dlg = new EditDialogTarget(ModuleLayoutType.Detail);
    var answer = dlg.ShowDialog(
        new DangerButton("削除"),
        new SecondaryOutlineButton("キャンセル"));
    Result.Value = "ShowDialog: " + answer;
}
