// 双方向 Id 持ち合いパターン:
// 親 (MutualMain) は SubSlot (ModuleField) で子 (MutualSub) を内包し、DB列 sub_id に子の Id を持つ。
// 子 (MutualSub) は Main (LinkField) で親を指し、DB列 main_id に親の Id を持つ。
// → 双方向サイクル。CLB の TemporaryIdResolver が片方を null Insert + 後追い UPDATE で解決する。
void OnAfterInitialization()
{
    if (IsNewData)
    {
        // 子の Main に「自分 (= 親) のテンポラリ Id」を設定。
        // Submit 時に親 ↔ 子 が双方向参照したまま保存され、Resolver がサイクル解決する。
        SubSlot.ChildModule.Main.Value = this.Id.Value;
    }
}
