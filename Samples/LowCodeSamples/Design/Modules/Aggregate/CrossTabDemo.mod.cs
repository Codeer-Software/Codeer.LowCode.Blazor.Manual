void Summary_OnCellClick(CrossTabCell cell)
{
    // クリックしたセルに数えた売上だけを明細に表示する
    Details.SetAdditionalCondition(cell.CreateSearcher());
    Details.Reload();
}
