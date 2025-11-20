
void 棚Aボタン_OnClick()
{
    棚Aボタン.IsViewOnly = true;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
    棚名.Value = "A";
}

void 棚Bボタン_OnClick()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = true;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚Cボタン_OnClick()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = true;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚Dボタン_OnClick()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = true;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚Eボタン_OnClick()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = true;
    棚Fボタン.IsViewOnly = false;
}

void 棚Fボタン_OnClick()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = true;
}

void 棚名_OnDataChanged()
{
    棚情報取得();
}

void 棚情報取得()
{
    if(棚名.Value == "A")
    {
        棚情報Aセット();
    }
}

void 棚情報Aセット()
{
    棚コード.Value = "TN-A";
    所属ゾーン.Value = "小物ピッキング";
    カテゴリ.Value = "A";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);
    取扱商品初期化();
    日用品.Value = true;
    常温.Value = true;
    小型.Value = true;
    液体.Value = true;
}

void 取扱商品初期化()
{
    日用品.Value = false;
    食品.Value = false;
    飲料.Value = false;
    美容ヘルス.Value = false;
    文具.Value = false;
    家電小物.Value = false;
    
    常温.Value = false;
    冷蔵.Value = false;
    冷凍.Value = false;
    
    小型.Value = false;
    中型.Value = false;
    長物.Value = false;
    割れ物.Value = false;
    液体.Value = false;
}