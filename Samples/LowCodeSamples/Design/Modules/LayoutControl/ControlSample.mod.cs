void DetailLayout_OnAfterInitialization()
{
    SetSample();
}

void SetSample()
{
    会社名.Value = "株式会社サンプルテック";
    担当者名.Value = "山本 一郎";
    社員数.Value = 1250;
    年間売上.Value = 4800000000;
    備考.Value = "毎月第 2 火曜日に定例会。\n請求書は PDF で送付。";
    契約日.Value = DateTime.Today.AddDays(-30);
    次回訪問.Value = DateTime.Today.AddDays(7).AddHours(14);
    業種.Value = "製造";
    企業規模.Value = "中堅企業";
    メルマガ配信.Value = true;
    重要顧客.Value = true;
    取引中.Value = true;
}

void 参照モード_OnDataChanged()
{
    var v = 参照モード.Value == true;
    会社名.IsViewOnly = v;
    担当者名.IsViewOnly = v;
    社員数.IsViewOnly = v;
    年間売上.IsViewOnly = v;
    備考.IsViewOnly = v;
    契約日.IsViewOnly = v;
    次回訪問.IsViewOnly = v;
    連絡可能時間.IsViewOnly = v;
    業種.IsViewOnly = v;
    大企業.IsViewOnly = v;
    中堅企業.IsViewOnly = v;
    中小企業.IsViewOnly = v;
    メルマガ配信.IsViewOnly = v;
    重要顧客.IsViewOnly = v;
    取引中.IsViewOnly = v;
    担当営業.IsViewOnly = v;
    ポータルパスワード.IsViewOnly = v;
    契約書.IsViewOnly = v;
    表示色.IsViewOnly = v;
    保存.IsVisible = !v;
    クリア.IsVisible = !v;
}

void 保存_OnClick()
{
    if (this.ValidateInput())
    {
        Toaster.Success("入力内容に問題はありません (デモのため保存はしません)");
    }
    else
    {
        Toaster.Error("入力内容を確認してください");
    }
}

void クリア_OnClick()
{
    会社名.Value = null;
    担当者名.Value = null;
    社員数.Value = null;
    年間売上.Value = null;
    備考.Value = null;
    契約日.Value = null;
    次回訪問.Value = null;
    業種.Value = null;
    企業規模.Value = null;
}
