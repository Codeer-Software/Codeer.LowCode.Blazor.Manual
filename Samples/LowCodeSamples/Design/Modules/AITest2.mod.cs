
void List_OnDataChanged()
{
    var total = 0;
    foreach(var e in this.List.Rows)
    {
        total += e.Amount.Value;
    }
    Total.Value = total;
}
void AITextAnalyzer_DataImportCompleted()
{
    List_OnDataChanged();
}