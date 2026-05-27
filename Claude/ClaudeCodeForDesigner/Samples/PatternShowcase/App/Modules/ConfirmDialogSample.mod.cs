void ShowInfo_OnClick()
{
    MessageBox.Show("保存が完了しました。", "OK");
    Result.Value = "情報: OK";
}

void AskDelete_OnClick()
{
    var answer = MessageBox.Show("このレコードを削除します。よろしいですか？", "削除", "キャンセル");
    Result.Value = "削除: " + answer;
}

void AskSave_OnClick()
{
    var answer = MessageBox.Show("編集中の内容があります。", "保存", "破棄", "キャンセル");
    Result.Value = "保存: " + answer;
}

void AskWithTitle_OnClick()
{
    var answer = MessageBox.ShowWithTitle("確認", "ログアウトしますか？", "はい", "いいえ");
    Result.Value = "ログアウト: " + answer;
}

void HtmlMessage_OnClick()
{
    var html = "<b>このレコードを削除します。</b><br/>"
        + "<span style=\"color:#d32f2f\">元に戻せません。</span><br/><br/>"
        + "<ul style=\"margin-bottom:0\">"
        + "<li>関連する明細データも一緒に削除されます</li>"
        + "<li>削除後の復元はできません</li>"
        + "</ul>";
    var answer = MessageBox.ShowWithTitle("削除の確認", html,
        new DangerButton("削除"),
        new SecondaryOutlineButton("キャンセル"));
    Result.Value = "HTML: " + answer;
}
