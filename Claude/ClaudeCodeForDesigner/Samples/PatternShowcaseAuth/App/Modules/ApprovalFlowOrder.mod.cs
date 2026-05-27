// 同じ Order の Members の状態を見て Order.Status を更新する
void UpdateAfterMemberChange()
{
    // 全 Members を ModuleSearcher で取得 (現状の Children を見る)
    int requiredCount = 0;
    int requiredApproved = 0;
    int optionalCount = 0;
    int optionalApproved = 0;
    int rejectedCount = 0;
    int totalCount = 0;
    foreach (var m in Members.Rows)
    {
        totalCount++;
        if (m.Status.Value == "Rejected") rejectedCount++;
        if (m.IsRequired.Value == true)
        {
            requiredCount++;
            if (m.Status.Value == "Approved") requiredApproved++;
        }
        else
        {
            optionalCount++;
            if (m.Status.Value == "Approved") optionalApproved++;
        }
    }

    if (rejectedCount > 0)
    {
        Status.Value = "Rejected";
        return;
    }
    // 必須が居る → 必須全員承認で完了
    if (requiredCount > 0)
    {
        if (requiredApproved == requiredCount) Status.Value = "Approved";
        return;
    }
    // 必須なし → 任意が1人でも承認すれば完了
    if (optionalCount > 0 && optionalApproved >= 1)
    {
        Status.Value = "Approved";
    }
}
