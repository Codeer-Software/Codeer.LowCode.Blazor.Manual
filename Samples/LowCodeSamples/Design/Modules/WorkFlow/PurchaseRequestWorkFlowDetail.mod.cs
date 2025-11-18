
void 数量_OnDataChanged()
{
    小計.Value = 数量.Value * 単価.Value;
}
void 単価_OnDataChanged()
{
    小計.Value = 数量.Value * 単価.Value;
}