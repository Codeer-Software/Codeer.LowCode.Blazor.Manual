
void 集計期間開始日_OnDataChanged()
{
    BubbleList.DateCange(集計期間開始日.Value, 集計期間終了日.Value);
}

void 集計期間終了日_OnDataChanged()
{
    BubbleList.DateCange(集計期間開始日.Value, 集計期間終了日.Value);
}

void ソート_OnDataChanged()
{
    BubbleList.SortChange(ソート.Value);
}