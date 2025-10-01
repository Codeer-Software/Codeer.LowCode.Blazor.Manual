
void 数量_OnDataChanged()
{
    合計金額計算();
}

void 単価_OnDataChanged()
{
    合計金額計算();
}

void 合計金額計算()
{
    合計.Value = 0;
    if(数量.Value != null && 単価.Value != null)
    {
        合計.Value = 数量.Value * 単価.Value;
    }
}