
void OnSearchInitialization()
{
    // ?delayed=true で遷移してきた場合は「遅延発注」フィルタを構築
    var queries = NavigationService.GetQueryParameters();
    if (!queries.ContainsKey("delayed")) return;
    if (queries["delayed"].Count == 0 || queries["delayed"][0] != "true") return;

    // ステータス: 発注中 OR 一部入庫
    var statuses = new List<string>();
    statuses.Add("発注中");
    statuses.Add("一部入庫");
    ステータス.SearchValues = statuses;

    // 希望納期: 今日以前（納期超過）
    希望納期.SearchMax = DateOnly.FromDateTime(DateTime.Now);
}

void OnAfterInitialization()
{
    // 新規作成時のデフォルト値
    if (this.IsNewData)
    {
        ステータス.Value = "仮登録";
        if (発注日.Value == null) 発注日.Value = DateTime.Now;
    }

    // ステータス別の表示制御
    var status = ステータス.Value;
    var isDraft = status == "仮登録";
    var isOrdered = status == "発注中";
    var isPartial = status == "一部入庫";
    var canReceive = isOrdered || isPartial;

    // 仮登録以外は備考と明細リスト以外を ViewOnly（備考はステータス問わず編集可）
    if (!isDraft)
    {
        発注日.IsViewOnly = true;
        希望納期.IsViewOnly = true;
        仕入先Id.IsViewOnly = true;
        倉庫Id.IsViewOnly = true;
        明細リスト.IsViewOnly = true;
    }

    // ボタン表示制御（登録ボタンは備考更新のため常時表示）
    登録ボタン.IsVisible = true;
    発注確定ボタン.IsVisible = isDraft;
    破棄ボタン.IsVisible = isDraft;
    キャンセルボタン.IsVisible = isOrdered;
    入庫登録ボタン.IsVisible = canReceive;
}

void 発注確定ボタン_OnClick()
{
    var msg = "この発注を確定します。<br>確定後は内容の修正ができなくなります。<br><br>発注確定しますか？";
    if (MessageBox.Show(msg, "確定", "キャンセル") != "確定") return;

    // 採番: 当日の最大連番を取得
    var today = DateTime.Now.ToString("yyyyMMdd");
    var prefix = "PO-" + today + "-";
    var search = new ModuleSearcher<発注>();
    search.AddLike(e => e.発注番号.Value, prefix);
    var results = search.Execute();

    int maxSeq = 0;
    foreach (var r in results)
    {
        var no = r.発注番号.Value;
        if (no == null) continue;
        var seqStr = no.Substring(prefix.Length);
        int.TryParse(seqStr, out var seq);
        if (seq > maxSeq) maxSeq = seq;
    }

    発注番号.Value = prefix + (maxSeq + 1).ToString().PadLeft(3, '0');
    ステータス.Value = "発注中";

    if (this.Submit()) Toaster.Success("発注を確定しました");
    else Toaster.Error("発注確定に失敗しました");
}

void キャンセルボタン_OnClick()
{
    var msg = "この発注をキャンセルします。<br>キャンセル後は内容の修正ができなくなります。<br><br>キャンセルしますか？";
    if (MessageBox.Show(msg, "キャンセルする", "戻る") != "キャンセルする") return;

    ステータス.Value = "キャンセル";

    if (this.Submit()) Toaster.Success("発注をキャンセルしました");
    else Toaster.Error("キャンセル処理に失敗しました");
}

void 破棄ボタン_OnClick()
{
    var msg = "この発注を破棄します。<br>破棄したデータは復元できません。<br><br>破棄しますか？";
    if (MessageBox.Show(msg, "破棄", "戻る") != "破棄") return;

    if (this.Delete())
    {
        Toaster.Success("発注を破棄しました");
        var url = NavigationService.GetModuleUrl("発注");
        NavigationService.NavigateTo(url);
    }
    else
    {
        Toaster.Error("破棄に失敗しました");
    }
}

void 入庫登録ボタン_OnClick()
{
    var newReceiving = new 入庫ダイアログ(ModuleLayoutType.Detail);
    newReceiving.入庫日.Value = DateTime.Now;
    newReceiving.倉庫Id.Value = 倉庫Id.Value;
    newReceiving.仕入先Id.Value = 仕入先Id.Value;
    newReceiving.備考.Value = "発注 " + (発注番号.Value ?? "") + " から入庫起こし";

    // 在庫発注明細 → ダイアログ行 への対応を index で保持
    var poIndexList = new List<int>();
    int addedCount = 0;
    for (int i = 0; i < 明細リスト.Rows.Count; i++)
    {
        var poDetail = (在庫発注明細)明細リスト.Rows[i];
        var ordered = poDetail.数量.Value ?? 0;
        var received = poDetail.入庫済数量.Value ?? 0;
        var remaining = ordered - received;
        if (remaining <= 0) continue;

        var newDetail = (入庫明細)newReceiving.明細リスト.AddRow();
        newDetail.商品コード.Value = poDetail.商品コード.Value;
        newDetail.数量.Value = remaining;
        newDetail.仕入単価.Value = poDetail.発注単価.Value;
        addedCount++;
        poIndexList.Add(i);
    }

    if (addedCount == 0) { Toaster.Warn("残数量がありません"); return; }

    var result = newReceiving.ShowDialog("キャンセル");
    if (result != "登録") return;

    // 入庫登録成功 → 在庫発注明細の入庫済数量を加算
    for (int i = 0; i < newReceiving.明細リスト.Rows.Count; i++)
    {
        var rd = (入庫明細)newReceiving.明細リスト.Rows[i];
        var poDetail = (在庫発注明細)明細リスト.Rows[poIndexList[i]];
        poDetail.入庫済数量.Value = (poDetail.入庫済数量.Value ?? 0) + (rd.数量.Value ?? 0);
    }

    // ステータス判定（全明細が入庫済 ≥ 発注数量 → 完了 / それ以外 → 一部入庫）
    bool allCompleted = true;
    foreach (var row in 明細リスト.Rows)
    {
        var pod = (在庫発注明細)row;
        if ((pod.入庫済数量.Value ?? 0) < (pod.数量.Value ?? 0)) { allCompleted = false; break; }
    }
    ステータス.Value = allCompleted ? "完了" : "一部入庫";

    if (this.Submit())
    {
        Toaster.Success("入庫を登録しました");
        this.Reload();
    }
    else
    {
        Toaster.Error("発注ステータスの更新に失敗しました");
    }
}
