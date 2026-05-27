void OnNew()
{
    if (IsNewData)
    {
        RecordDate.Value = DateTime.Today;
        CreatedByName.Value = "ログインユーザー";
    }
}
