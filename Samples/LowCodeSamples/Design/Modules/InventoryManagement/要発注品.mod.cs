
void 一括発注ボタン_OnClick()
{
    var msg = "要発注品を仕入先×倉庫別にグループ化して<br>仮登録の発注書を一括作成します。<br><br>実行しますか？";
    if (MessageBox.Show(msg, "実行", "キャンセル") != "実行") return;

    var search = new ModuleSearcher<q_要発注品>();
    var results = search.Execute();

    if (results.Count == 0)
    {
        Toaster.Warn("対象の要発注品がありません");
        return;
    }

    // 仕入先 × 倉庫 でグループ化
    var groups = new Dictionary<string, List<q_要発注品>>();
    int skippedCount = 0;
    foreach (var item in results)
    {
        var data = (q_要発注品)item;
        if (data.仕入先Id.Value == null)
        {
            skippedCount++;
            continue;
        }
        var key = data.仕入先Id.Value + "_" + data.倉庫Id.Value;
        if (!groups.ContainsKey(key))
        {
            groups[key] = new List<q_要発注品>();
        }
        groups[key].Add(data);
    }

    int createdCount = 0;
    foreach (var pair in groups)
    {
        var items = pair.Value;
        var first = items[0];

        var newOrder = new 発注();
        newOrder.発注日.Value = DateTime.Now;
        newOrder.希望納期.Value = DateTime.Now.AddDays(14);
        newOrder.仕入先Id.Value = first.仕入先Id.Value;
        newOrder.倉庫Id.Value = first.倉庫Id.Value;
        newOrder.ステータス.Value = "仮登録";

        foreach (var data in items)
        {
            var detail = (在庫発注明細)newOrder.明細リスト.AddRow();
            detail.商品コード.Value = data.商品コード.Value;
            detail.数量.Value = data.推奨数量.Value;
        }

        if (newOrder.Submit()) createdCount++;
    }

    if (skippedCount > 0)
    {
        Toaster.Warn(createdCount + "件の発注を作成しました（仕入先未設定 " + skippedCount + "件はスキップ）");
    }
    else
    {
        Toaster.Success(createdCount + "件の発注を作成しました");
    }

    var url = NavigationService.GetModuleUrl("発注");
    NavigationService.NavigateTo(url);
}
