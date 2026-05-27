void Search_OnSearched()
{
    UpdateSummary();
}

void ShowCountButton_OnClick()
{
    MessageBox.Show("検索結果は " + Results.RowCount + " 件です");
}

void ReloadButton_OnClick()
{
    Results.Reload();
    UpdateSummary();
}

void UpdateSummary()
{
    int count = Results.RowCount;
    decimal totalStock = 0;
    decimal totalPrice = 0;
    foreach (var row in Results.Rows)
    {
        totalStock += row.Stock.Value;
        totalPrice += row.Price.Value;
    }
    decimal avg = count > 0 ? totalPrice / count : 0;
    SummaryText.Text = "件数: " + count + "件　合計在庫: " + totalStock + "　平均価格: " + (int)avg + " 円";
}
