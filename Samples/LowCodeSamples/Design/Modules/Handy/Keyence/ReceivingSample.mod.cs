
void DetailLayoutDesign_OnAfterInitialization()
{
    入荷日.Value = DateOnly.FromDateTime(DateTime.Now);
}

void SKU_OnDataChanged()
{
    アイテムリスト.DeleteAllRows();
    if(string.IsNullOrEmpty(SKU.Value)) return;
    
    using var suspend = SuspendNotifyStateChanged();
    var row = アイテムリスト.AddRow();
    row.SKU.Value = SKU.Value;
    row.ロケーション.Value = "第一倉庫";
    row.アイテムコード.Value = "ITM-0001";
    row.アイテム名.Value = "キーボード";
    row.在庫数.Value = 50;
    
    row = アイテムリスト.AddRow();
    row.SKU.Value = SKU.Value;
    row.ロケーション.Value = "第二倉庫";
    row.アイテムコード.Value = "ITM-0001";
    row.アイテム名.Value = "キーボード";
    row.在庫数.Value = 100;
}

void クリアボタン_OnClick()
{
    入荷日.Value = null;
    SKU.Value = null;
    ロケーション.Value = null;
    数量.Value = null;
    メモ.Value = null;
}

void 入荷ボタン_OnClick()
{
    Toaster.Warn("DBと接続してないので保存できません");
}