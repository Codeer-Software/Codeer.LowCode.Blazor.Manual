
void 単価_OnDataChanged()
{
    合計.Value = 単価.Value * 数量.Value;
}
void 数量_OnDataChanged()
{
    合計.Value = 単価.Value * 数量.Value;
}