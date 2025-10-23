
void DetailLayoutDesign_OnAfterInitialization()
{
    想定リードタイム.Value = "3-10営業日（目安）";
    申請者.Value = "山田太郎";
    部門.Value = "経理";
    メール.Value = "yamada@sample";
    通貨.Value = "JPY";
    予算コード.Value = "YSM-2025-AB123";
    カテゴリ.Value = "ソフトウェア";
    
    申請者.IsEnabled = false;
    部門.IsEnabled = false;
    メール.IsEnabled = false;
    予算コード.IsEnabled = false;
    
    var row = 発注明細リスト.AddRow();
}

void 追加ボタン_OnClick()
{
    var row = 発注明細リスト.AddRow();
}

void 行クリアボタン_OnClick()
{
    発注明細リスト.DeleteAllRows();
}

void 発注明細リスト_OnDataChanged()
{
    金額計算();
}

void 税率_OnDataChanged()
{
    if(税率.Value == null || 税率.Value <= 0)
    {
        税額.Value = 0;
    }
    金額計算();
}

void 金額計算()
{
    小計.Value = 0;
    税額.Value = 0;
    合計.Value = 0;
    
    foreach(var i in 発注明細リスト.Rows)
    {
        小計.Value += i.小計.Value;
    }
    
    if(税率.Value != null || 0 < 税率.Value)
    {
        税額.Value =  Math.Floor(小計.Value * (税率.Value/100));
    }
    
    合計.Value = 小計.Value + 税額.Value;
}

void 申請ボタン_OnClick()
{
    Toaster.Error("デモサイトのため保存できません");
}

void 下書き保存ボタン_OnClick()
{
    Toaster.Error("デモサイトのため保存できません");
}