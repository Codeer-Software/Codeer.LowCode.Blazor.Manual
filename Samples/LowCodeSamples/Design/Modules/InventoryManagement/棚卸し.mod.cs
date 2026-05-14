
void 登録ボタン_OnClick()
{
    var msg = "棚卸しは実在庫数で在庫を上書きします。<br>他ユーザーが同じ商品の入出庫・棚卸しを<br>並行して登録していた場合は失敗します。<br><br>登録しますか？";
    if (MessageBox.Show(msg, "実行", "キャンセル") != "実行") return;
    if (this.Submit()) Toaster.Success("棚卸しを登録しました");
    else Toaster.Error("棚卸しの登録に失敗しました");
}
