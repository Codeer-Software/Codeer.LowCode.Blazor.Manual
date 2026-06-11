void InitSearch()
{
    // 単一値: SelectField / BooleanField / TextField / LinkField は SearchValue
    Status.SearchValue = "InProgress";
    IsActive.SearchValue = true;

    // 範囲 (RangeSearchField): Number / Date / DateTime / Time は SearchMin / SearchMax
    Price.SearchMin = 100;
    Price.SearchMax = 1000;
    DueDate.SearchMin = DateOnly.FromDateTime(DateTime.Today);
    DueDate.SearchMax = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
}
