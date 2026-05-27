// ============================================================
// 新規申請時の初期化 (親モジュールの OnAfterInitialization から呼ぶ)
// parentId は親の @temporary:guid をそのまま入れて良い。
// 親 ↔ ApprovalFlow の双方向参照は CLB の TemporaryIdResolver がサイクル解決する。
// ============================================================
void Initialize(string parentModuleName, string parentId, string templateName)
{
    Status.Value = "Pending";
    AttemptNo.Value = 1;
    ParentModuleName.Value = parentModuleName;
    ParentId.Value = parentId;

    var s = new ModuleSearcher<ApprovalFlowTemplate>();
    s.AddEquals(t => t.Name.Value, templateName);
    var tmpls = s.Execute();
    if (tmpls.Count > 0) TemplateId.Value = tmpls[0].Id.Value;
}

// 申請ボタン押下時に Orders/Members をテンプレから生成する。
// UseIndexSort=true なので OrderNo は Submit 時に 0,1,... で自動採番される。
// ここでは最初の Order を Active に、それ以外を Waiting にするだけ。
void LoadFromTemplate()
{
    if (TemplateId.Value == null) return;
    var tmplSearcher = new ModuleSearcher<ApprovalFlowTemplateOrder>();
    tmplSearcher.AddEquals(o => o.TemplateId.Value, TemplateId.Value);
    tmplSearcher.OrderBy(o => o.OrderNo.Value);
    var tmplOrders = tmplSearcher.Execute();

    var first = true;
    foreach (var tmplOrder in tmplOrders)
    {
        var newOrder = Orders.AddRow();
        newOrder.Status.Value = first ? "Active" : "Waiting";
        first = false;

        var memberSearcher = new ModuleSearcher<ApprovalFlowTemplateMember>();
        memberSearcher.AddEquals(m => m.TemplateOrderId.Value, tmplOrder.Id.Value);
        var tmplMembers = memberSearcher.Execute();
        foreach (var tmplMember in tmplMembers)
        {
            var newMember = newOrder.Members.AddRow();
            newMember.IsRequired.Value = tmplMember.IsRequired.Value;
            newMember.ApproverUser.Value = tmplMember.ApproverUser.Value;
            newMember.Status.Value = "Waiting";
            newMember.ParentModuleName.Value = ParentModuleName.Value;
            newMember.ParentId.Value = ParentId.Value;
        }
    }
    RecalculateCurrentApproverDisplay();
}

// 現在 Active な Order の最初の Waiting Member の承認者を CurrentApprover にセット
// (複数並列のときは最初の 1 人。検索しやすさ重視で代表者を持つ)
void RecalculateCurrentApproverDisplay()
{
    foreach (var o in Orders.Rows)
    {
        if (o.Status.Value != "Active") continue;
        foreach (var m in o.Members.Rows)
        {
            if (m.Status.Value != "Waiting") continue;
            CurrentApprover.Value = m.ApproverUser.Value;
            return;
        }
    }
    CurrentApprover.Value = null;
}

// 承認フローを「状態：進行中 申請者→(B,C)→▶D→E」形式の1行文字列にする
// マーカー: ✓=承認済 / ✗=却下 / —=スキップ / ▶=現在の担当 / (空)=未着手
// UseIndexSort=true なので Orders.Rows は OrderNo 昇順保証 → ソート不要
void UpdateFlowSummary()
{
    // 新規申請時は Id が @temporary:guid なので ModuleSearcher で数値列にぶつけるとエラー
    if (GetParentModule().IsNewData)
    {
        FlowSummary.Value = "";
        return;
    }

    var parts = new List<string>();

    // 先頭に申請者 (履歴の最古 Submit から取る — 親の LinkField は遅延ロードで取れないため)
    var hs = new ModuleSearcher<ApprovalHistory>();
    hs.AddEquals(h => h.ApprovalFlowId.Value, this.Id.Value);
    hs.AddEquals(h => h.Action.Value, "Submit");
    var subHistory = hs.Execute();
    if (subHistory.Count > 0)
    {
        var creatorName = subHistory[0].ActorUser.DisplayText;
        if (!string.IsNullOrEmpty(creatorName)) parts.Add("✓" + creatorName);
    }

    // 各 Order のメンバー (Order.Status は遅延ロードで取れないことがあるので「最初の Waiting に ▶」戦略)
    var currentMarked = false;
    foreach (var o in Orders.Rows)
    {
        var names = new List<string>();
        foreach (var m in o.Members.Rows)
        {
            var ms = m.Status.Value;
            var mark = "";
            if (ms == "Approved") mark = "✓";
            else if (ms == "Rejected") mark = "✗";
            else if (ms == "Skipped") mark = "—";
            else if (!currentMarked) { mark = "▶"; currentMarked = true; }
            names.Add(mark + m.ApproverUser.DisplayText);
        }
        if (names.Count == 0) continue;
        if (names.Count == 1) parts.Add(names[0]);
        else parts.Add("(" + string.Join(",", names) + ")");
    }

    var flowStr = string.Join("→", parts);
    var statusDisplay = Status.DisplayText;
    if (string.IsNullOrEmpty(statusDisplay)) statusDisplay = Status.Value;
    FlowSummary.Value = "状態:" + statusDisplay + "  " + flowStr;
}

// 承認待ち一覧の検索初期値: 現在の承認者 = 自分
void OnSearchInitialization()
{
    CurrentApprover.SearchValue = CurrentUser.Id.Value;
}

// 承認待ち一覧の「開く」ボタン: 各申請モジュール (LeaveRequest / ExpenseRequest) に遷移。
// ListField 経由の行 Module は LinkField/TextField の .Value が遅延ロードで空のことがあるので、
// 自分の Id で ModuleSearcher 再取得して値を確実に取る。
void OpenRequest_OnClick()
{
    var s = new ModuleSearcher<ApprovalFlow>();
    s.AddEquals(f => f.Id.Value, Id.Value);
    var rs = s.Execute();
    if (rs.Count == 0) return;
    var parentModule = rs[0].ParentModuleName.Value;
    var parentId = rs[0].ParentId.Value;
    if (string.IsNullOrEmpty(parentModule) || string.IsNullOrEmpty(parentId)) return;
    NavigationService.NavigateTo(NavigationService.GetModuleDataUrl(parentModule, parentId));
}

// 現在ユーザーが申請者か (parent.Creator.Value は LinkField 遅延ロードで取れないため履歴ベース)
bool IsCurrentUserCreator()
{
    var hs = new ModuleSearcher<ApprovalHistory>();
    hs.AddEquals(h => h.ApprovalFlowId.Value, this.Id.Value);
    hs.AddEquals(h => h.Action.Value, "Submit");
    var subHistory = hs.Execute();
    if (subHistory.Count == 0) return false;
    return subHistory[0].ActorUser.Value == CurrentUser.Id.Value;
}

// ============================================================
// 履歴ヘルパー
// ============================================================
void AddHistory(string action, string comment)
{
    var h = History.AddRow();
    h.AttemptNo.Value = AttemptNo.Value;
    h.ActorUser.Value = CurrentUser.Id.Value;
    h.Action.Value = action;
    h.ActedAt.Value = DateTime.Now;
    h.Comment.Value = comment;
}

// Reject/Cancel 等で残りの Order/Member を Skipped にする
void SkipRemainingOrdersAndMembers()
{
    foreach (var o in Orders.Rows)
    {
        if (o.Status.Value == "Waiting" || o.Status.Value == "Active")
            o.Status.Value = "Skipped";
        foreach (var m in o.Members.Rows)
        {
            if (m.Status.Value == "Waiting") m.Status.Value = "Skipped";
        }
    }
}

// ============================================================
// 初期化 + ボタン出し分け
// ============================================================
void OnAfterInitialization()
{
    UpdateFlowSummary();
    UpdateButtons();
}

// UI 側: 現在ユーザー宛の Waiting Member がいるか DB から判定 (Order.Id 経由)
bool HasWaitingMemberForCurrentUser()
{
    var parent = GetParentModule();
    if (parent.IsNewData) return false;
    foreach (var o in Orders.Rows)
    {
        var s = new ModuleSearcher<ApprovalFlowMember>();
        s.AddEquals(m => m.ApprovalFlowOrderId.Value, o.Id.Value);
        s.AddEquals(m => m.ApproverUser.Value, CurrentUser.Id.Value);
        s.AddEquals(m => m.Status.Value, "Waiting");
        if (s.Execute().Count > 0) return true;
    }
    return false;
}

// Approve/Reject 実行時: Order.Id 経由で DB から自分宛の Waiting Member を検索 + メモリ Member を返す
ApprovalFlowMember GetCurrentMemberForUserStrict()
{
    foreach (var o in Orders.Rows)
    {
        var s = new ModuleSearcher<ApprovalFlowMember>();
        s.AddEquals(m => m.ApprovalFlowOrderId.Value, o.Id.Value);
        s.AddEquals(m => m.ApproverUser.Value, CurrentUser.Id.Value);
        s.AddEquals(m => m.Status.Value, "Waiting");
        var members = s.Execute();
        if (members.Count == 0) continue;
        var dbMember = members[0];
        foreach (var m in o.Members.Rows)
        {
            if (m.Id.Value == dbMember.Id.Value) return m;
        }
    }
    return null;
}

// ボタン可視性更新
void UpdateButtons()
{
    var parent = GetParentModule();
    var isNewParent = parent.IsNewData;
    var s = Status.Value;
    var isPending = s == "Pending";
    var isRejected = s == "Rejected";
    var isCancelled = s == "Cancelled";
    var canApprove = isPending && HasWaitingMemberForCurrentUser();
    var isCreator = !isNewParent && IsCurrentUserCreator();

    SubmitButton.IsVisible   = isNewParent;
    ApproveButton.IsVisible  = !isNewParent && canApprove;
    RejectButton.IsVisible   = !isNewParent && canApprove;
    ResubmitButton.IsVisible = !isNewParent && (isRejected || isCancelled) && isCreator;
    CancelButton.IsVisible   = !isNewParent && isPending && isCreator;

    Comment.IsEnabled = canApprove;
}

// ============================================================
// 申請ボタン
// ============================================================
void SubmitButton_OnClick()
{
    var parent = GetParentModule();
    var wasNew = parent.IsNewData;

    using var suspend = parent.SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    if (wasNew)
    {
        LoadFromTemplate();
        AddHistory("Submit", "");
    }

    var ret = parent.Submit();
    if (ret != true) { Toaster.Error("申請に失敗しました"); return; }
    Toaster.Success("申請しました");
}

// ============================================================
// 承認ボタン
// ============================================================
void Approve_OnClick()
{
    using var suspend = GetParentModule().SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    var member = GetCurrentMemberForUserStrict();
    if (member == null) { Toaster.Error("承認権限がありません"); return; }

    member.Status.Value = "Approved";
    member.ActorUser.Value = CurrentUser.Id.Value;
    member.ApprovedAt.Value = DateTime.Now;

    var order = GetOrderById(member.ApprovalFlowOrderId.Value);
    if (order != null && IsOrderCompleted(order))
    {
        order.Status.Value = "Approved";
        AdvanceToNextOrder();
    }

    AddHistory("Approve", Comment.Value);
    Comment.Value = "";
    RecalculateCurrentApproverDisplay();
    UpdateFlowSummary();

    var ret = GetParentModule().Submit();
    if (ret == true) Toaster.Success("承認しました");
}

ApprovalFlowOrder GetOrderById(string orderId)
{
    foreach (var o in Orders.Rows)
        if (o.Id.Value == orderId) return o;
    return null;
}

// Order の承認完了判定: 必須メンバー全員 Approved、または必須ゼロで誰か1人 Approved
bool IsOrderCompleted(ApprovalFlowOrder order)
{
    int requiredCount = 0, requiredApproved = 0, optionalApproved = 0;
    foreach (var m in order.Members.Rows)
    {
        if (m.IsRequired.Value == true)
        {
            requiredCount++;
            if (m.Status.Value == "Approved") requiredApproved++;
        }
        else
        {
            if (m.Status.Value == "Approved") optionalApproved++;
        }
    }
    if (requiredCount > 0) return requiredApproved == requiredCount;
    return optionalApproved >= 1;
}

// 次の Waiting な Order を Active 化。なければフロー全体を Approved に。
// UseIndexSort=true で Orders.Rows は OrderNo 昇順保証なので、最初の Waiting が次の承認対象。
void AdvanceToNextOrder()
{
    foreach (var o in Orders.Rows)
    {
        if (o.Status.Value == "Waiting")
        {
            o.Status.Value = "Active";
            return;
        }
    }
    Status.Value = "Approved";
    CurrentApprover.Value = null;
}

// ============================================================
// 却下ボタン
// ============================================================
void Reject_OnClick()
{
    using var suspend = GetParentModule().SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    var member = GetCurrentMemberForUserStrict();
    if (member == null) { Toaster.Error("却下権限がありません"); return; }

    member.Status.Value = "Rejected";
    member.ActorUser.Value = CurrentUser.Id.Value;
    member.ApprovedAt.Value = DateTime.Now;

    var order = GetOrderById(member.ApprovalFlowOrderId.Value);
    if (order != null) order.Status.Value = "Rejected";

    Status.Value = "Rejected";
    SkipRemainingOrdersAndMembers();
    AddHistory("Reject", Comment.Value);
    Comment.Value = "";
    RecalculateCurrentApproverDisplay();
    UpdateFlowSummary();

    var ret = GetParentModule().Submit();
    if (ret == true) Toaster.Info("却下しました");
}

// ============================================================
// 再申請ボタン
// ============================================================
void Resubmit_OnClick()
{
    if (Status.Value != "Rejected" && Status.Value != "Cancelled") return;
    var parent = GetParentModule();

    using var suspend = parent.SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    if (!IsCurrentUserCreator()) { Toaster.Error("自分の申請のみ再申請できます"); return; }

    AttemptNo.Value = (AttemptNo.Value ?? 0) + 1;
    Status.Value = "Pending";

    // 古い Orders を削除してテンプレから再構築
    var rowsToDelete = new List<ApprovalFlowOrder>();
    foreach (var o in Orders.Rows) rowsToDelete.Add(o);
    foreach (var o in rowsToDelete) Orders.DeleteRow(o);

    var tmplName = parent.SelectTemplateName();
    var ts = new ModuleSearcher<ApprovalFlowTemplate>();
    ts.AddEquals(t => t.Name.Value, tmplName);
    var tmpls = ts.Execute();
    if (tmpls.Count == 0) { Toaster.Error("テンプレートが見つかりません"); return; }
    TemplateId.Value = tmpls[0].Id.Value;
    LoadFromTemplate();
    AddHistory("Resubmit", "");

    var ret = parent.Submit();
    if (ret == true) Toaster.Success("再申請しました");
    else if (ret == false) Toaster.Error("再申請に失敗しました");
}

// ============================================================
// キャンセルボタン
// ============================================================
void Cancel_OnClick()
{
    using var suspend = GetParentModule().SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    if (Status.Value != "Pending") return;
    var parent = GetParentModule();
    if (!IsCurrentUserCreator()) { Toaster.Error("自分の申請のみキャンセルできます"); return; }

    Status.Value = "Cancelled";
    SkipRemainingOrdersAndMembers();
    AddHistory("Cancel", "申請をキャンセルしました");
    RecalculateCurrentApproverDisplay();
    UpdateFlowSummary();

    var ret = parent.Submit();
    if (ret == true) Toaster.Success("キャンセルしました");
}
