
void DetailLayoutDesign_OnAfterInitialization()
{
    車価格.Value = 3600000;
    棚名.Value = "A";
}

void 棚Aボタン_OnClick()
{
    棚A選択();
    棚名.Value = "A";
}

void 棚Bボタン_OnClick()
{
    棚B選択();
    棚名.Value = "B";
}

void 棚Cボタン_OnClick()
{
    棚C選択();
    棚名.Value = "C";
}

void 棚Dボタン_OnClick()
{
    棚D選択();
    棚名.Value = "D";
}

void 棚Eボタン_OnClick()
{
    棚E選択();
    棚名.Value = "E";
}

void 棚Fボタン_OnClick()
{
    棚F選択();
    棚名.Value = "F";
}

void 棚名_OnDataChanged()
{
    棚情報取得();
}

void 棚情報取得()
{
    if (棚名.Value == "A")
    {
        棚A選択();
        棚情報Aセット();
    }
    else if (棚名.Value == "B")
    {
        棚B選択();
        棚情報Bセット();
    }
    else if (棚名.Value == "C")
    {
        棚C選択();
        棚情報Cセット();
    }
    else if (棚名.Value == "D")
    {
        棚D選択();
        棚情報Dセット();
    }
    else if (棚名.Value == "E")
    {
        棚E選択();
        棚情報Eセット();
    }
    else if (棚名.Value == "F")
    {
        棚F選択();
        棚情報Fセット();
    }

}

void 棚A選択()
{
    棚Aボタン.IsViewOnly = true;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚B選択()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = true;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚C選択()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = true;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚D選択()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = true;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = false;
}

void 棚E選択()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = true;
    棚Fボタン.IsViewOnly = false;
}

void 棚F選択()
{
    棚Aボタン.IsViewOnly = false;
    棚Bボタン.IsViewOnly = false;
    棚Cボタン.IsViewOnly = false;
    棚Dボタン.IsViewOnly = false;
    棚Eボタン.IsViewOnly = false;
    棚Fボタン.IsViewOnly = true;
}

void 棚情報Aセット()
{
    棚コード.Value = "TN-A";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);
    取扱商品初期化();
    日用品.Value = true;
    常温.Value = true;
    小型.Value = true;
    液体.Value = true;
    メモ.Value = "日用品・消耗品の小型常温商品用\r\n例）洗剤・ティッシュなど";
}

void 棚情報Bセット()
{
    棚コード.Value = "TN-B";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);

    取扱商品初期化();
    食品.Value = true;
    飲料.Value = true;

    常温.Value = true;

    小型.Value = true;
    液体.Value = true;
    メモ.Value = "食品・飲料の小型常温商品用\r\n例）菓子・ペットボトル飲料など";
}

void 棚情報Cセット()
{
    棚コード.Value = "TN-C";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);

    取扱商品初期化();
    美容ヘルス.Value = true;

    常温.Value = true;

    小型.Value = true;
    割れ物.Value = true;
    液体.Value = true;
    メモ.Value = "美容・ヘルス系の小型常温商品用\r\n例）化粧品・スキンケアなど（割れ物・液体多め）";
}

void 棚情報Dセット()
{
    棚コード.Value = "TN-D";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);

    取扱商品初期化();
    家電小物.Value = true;

    常温.Value = true;

    小型.Value = true;
    中型.Value = true;
    メモ.Value = "家電小物の小型〜中型常温商品用\r\n例）ケーブル・電池・アダプタなど";
}

void 棚情報Eセット()
{
    棚コード.Value = "TN-E";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);

    取扱商品初期化();
    文具.Value = true;

    常温.Value = true;

    小型.Value = true;
    メモ.Value = "文具・オフィス用品の小型常温商品用\r\n例）ペン・ノート・付箋など";
}

void 棚情報Fセット()
{
    棚コード.Value = "TN-F";
    所属ゾーン.Value = "小物ピッキング";
    棚有効無効.Value = true;
    運用開始日.Value = DateOnly.FromDateTime(DateTime.Now);
    次回棚卸日時.Value = 運用開始日.Value.AddDays(20);

    取扱商品初期化();
    日用品.Value = true;
    食品.Value = true;
    飲料.Value = true;
    美容ヘルス.Value = true;
    文具.Value = true;
    家電小物.Value = true;

    常温.Value = true;

    小型.Value = true;
    中型.Value = true;
    メモ.Value = "シーズン・キャンペーン用の混在棚\r\n小型〜中型の常温商品全般を想定";
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

void 外装カスタマイズブール_OnDataChanged()
{
    車価格取得();
}

void 内装カスタマイズブール_OnDataChanged()
{
    車価格取得();
}

void 安全運転支援パックブール_OnDataChanged()
{
    車価格取得();
}

void 車価格取得()
{
    車価格.Value = 3600000;
    if(外装カスタマイズブール.Value)
    {
        車価格.Value += 3500000;
    }
    
    if(内装カスタマイズブール.Value)
    {
        車価格.Value += 230000;
    }
    
    if(安全運転支援パックブール.Value)
    {
        車価格.Value += 150000;
    }
}

void 予約ボタン_OnClick()
{
    予約情報グリッド.IsExpanded = true;
    予約情報_車サイズ.Value = "全長 4,650mm\r\n全幅 1,850mm";
    予約情報_乗車定員.Value = "5-7人";
    予約情報_燃費.Value = "ハイブリッド 21km/L";
    
    var 本体価格 = 3600000;
    var 諸費用 = 180000;
    
    var 外装 = 0;
    var 内装 = 0;
    var 安全 = 0;
    
    予約情報_外装カスタマイズブール.Value = 外装カスタマイズブール.Value;
    予約情報_内装カスタマイズブール.Value = 内装カスタマイズブール.Value;
    予約情報_安全運転支援パックブール.Value = 安全運転支援パックブール.Value;
    
    if(予約情報_外装カスタマイズブール.Value)
    {
        外装 = 3500000;
    }
    
    if(予約情報_内装カスタマイズブール.Value)
    {
        内装 = 230000;
    }
    
    if(予約情報_安全運転支援パックブール.Value)
    {
        安全 = 150000;
    }
    
    見積.Value = "車両本体：" + 本体価格.ToString("C")
                 + "\r\n" + "カスタマイズ：" + "外装・・・" + 外装.ToString("C")
                 + "\r\n" + "　　　　　　　内装・・・" + 内装.ToString("C")
                 + "\r\n" + "　　　　　　　安全運転支援パック・・・" + 安全.ToString("C")
                 + "\r\n" + "諸費用：" + 諸費用.ToString("C");
    
    var 合計 = 本体価格 + 諸費用 + 外装 + 内装 + 安全;
    見積金額.Value = 合計;
}