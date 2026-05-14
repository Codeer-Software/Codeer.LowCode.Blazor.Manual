
void 商品コード_OnDataChanged()
{
    if (this.IsNewData)
    {
        商品Id.Value = 商品コード.Id.Value;
        商品名.Value = 商品コード.品名.Value;

        // 在庫を (商品Id, 親棚卸しの倉庫Id) で検索して 在庫モジュールにロード
        var parent = (棚卸し)this.GetParentModule();
        var warehouseId = parent.倉庫Id.Value;
        var search = new ModuleSearcher<在庫>();
        search.AddEquals(e => e.商品Id.Value, 商品Id.Value);
        search.AddEquals(e => e.倉庫Id.Value, warehouseId);
        var results = search.Execute();

        if (results.Count == 0)
        {
            // 既存在庫なし - 新規モード（在庫レコードが作られる）
            var child = (在庫)在庫モジュール.ChildModule;
            child.商品Id.Value = 商品Id.Value;
            child.倉庫Id.Value = warehouseId;
            child.現在庫数.Value = 0;
            帳簿在庫数.Value = 0;
        }
        else
        {
            // 既存在庫あり - version も丸ごと引き継いで楽観ロックを効かせる
            var inv = (在庫)results[0];
            在庫モジュール.ChildModule = inv;
            帳簿在庫数.Value = inv.現在庫数.Value;
        }
    }
}

void 帳簿在庫数_OnDataChanged()
{
    差分計算();
}

void 実在庫数_OnDataChanged()
{
    差分計算();

    // 在庫モジュールの現在庫数にも伝播（Submit 時に在庫が更新される）
    if (在庫モジュール.ChildModule != null)
    {
        var child = (在庫)在庫モジュール.ChildModule;
        child.現在庫数.Value = 実在庫数.Value ?? 0;
    }
}

void 差分計算()
{
    var book = 帳簿在庫数.Value ?? 0;
    var actual = 実在庫数.Value ?? 0;
    差分.Value = actual - book;
}
