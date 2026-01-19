
void 集計期間開始日_OnDataChanged()
{
    var component = (BubbleListProCodeComponent)プロコード.Component;
    component.OnStartInput(集計期間開始日.Value);
}

void 集計期間終了日_OnDataChanged()
{
    var component = (BubbleListProCodeComponent)プロコード.Component;
    component.OnEndInput(集計期間終了日.Value);
}

void ソート_OnDataChanged()
{
    var component = (BubbleListProCodeComponent)プロコード.Component;
    component.OnSortChange(ソート.Value);
}