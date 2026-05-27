// LeaveType をベースにテンプレートを選択 (ApprovalFlow から呼ばれる)
string SelectTemplateName()
{
    return LeaveType.Value == "special" ? "FullLeave" : "SimpleLeave";
}

void OnAfterInitialization()
{
    if (IsNewData)
    {
        // 新規時: ApprovalFlow を初期化。this.Id.Value は @temporary:guid だが、
        // CLB の TemporaryIdResolver が双方向サイクルを自動解決する。
        ApprovalFlow.ChildModule.Initialize("LeaveRequest", this.Id.Value, SelectTemplateName());
        return;
    }

    // 申請後 (新規でない) は申請内容を変更不可。却下/キャンセル時のみ再申請のため編集可。
    var flowStatus = ApprovalFlow.ChildModule.Status.Value;
    var reopenable = (flowStatus == "Rejected" || flowStatus == "Cancelled");
    EditableGrid.IsEnabled = reopenable;
}
