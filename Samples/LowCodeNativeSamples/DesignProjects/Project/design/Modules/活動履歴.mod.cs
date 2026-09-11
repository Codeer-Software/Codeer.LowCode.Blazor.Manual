
void 種別_OnSearchDataChanged()
{
    検索実行();
}
void 担当者Id_OnSearchDataChanged()
{
    検索実行();
}
void 内容メモ_OnSearchDataChanged()
{
    検索実行();
}

void 検索実行()
{
    var serach = (SearchField)GetParentField();
    serach.ExecuteSearch();
}

void 編集_OnDataChanged()
{
    this.IsViewOnly = !this.IsViewOnly;
}