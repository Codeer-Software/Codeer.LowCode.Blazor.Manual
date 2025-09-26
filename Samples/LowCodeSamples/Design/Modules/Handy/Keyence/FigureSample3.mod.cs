
void ボタン_OnClick()
{
    Toaster.Warn("DBと接続してないので保存できません");
}

void Marker_OnClick(int x, int y)
{
    var dlg = new InputNumberPopup();
    dlg.基準.Text = "基準 4.5 ± 0.5";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}
void Marker2_OnClick(int x, int y)
{
    var dlg = new InputNumberPopup();
    dlg.基準.Text = "基準 21.5 ± 0.5";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}
void Marker3_OnClick(int x, int y)
{
    var dlg = new InputNumberPopup();
    dlg.基準.Text = "基準 3.5 ± 0.5";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}
void Marker4_OnClick(int x, int y)
{
    var dlg = new InputNumberPopup();
    dlg.基準.Text = "基準 7.5 ± 0.5";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}
void Marker5_OnClick(int x, int y)
{
    var dlg = new InputNumberPopup();
    dlg.基準.Text = "基準 90° ± 1.0";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}
void Marker6_OnClick(int x, int y)
{
    var dlg = new SelectOkNgPopup();
    dlg.基準.Text = "基準 4.5 ± 0.5";
    dlg.ShowPopup(x, y, "OK", "キャンセル");
}