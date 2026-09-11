
void 活動履歴追加ボタン_OnClick()
{
    var dlg = new 活動履歴(ModuleLayoutType.Detail, "");
    dlg.商談Id.Value = this.Id.Value;
    if (dlg.ShowDialog("OK", "Cancel") != "OK") return;

    var row = 活動履歴リスト.AddRow();
    row.種別.Value = dlg.種別.Value;
    row.活動日時.Value = dlg.活動日時.Value;
    row.担当者Id.Value = dlg.担当者Id.Value;
    row.やり取り相手Id.Value = dlg.やり取り相手Id.Value;
    row.内容メモ.Value = dlg.内容メモ.Value;
}

void 顧客追加ボタン_OnClick()
{
    var dlg = new 顧客(ModuleLayoutType.Detail, "");
    dlg.ShowDialog("OK");
}