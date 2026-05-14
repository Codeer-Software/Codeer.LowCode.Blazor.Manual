
void 商品コード_OnDataChanged()
{
    if (this.IsNewData)
    {
        商品Id.Value = 商品コード.Id.Value;
        商品名.Value = 商品コード.品名.Value;
        仕入単価.Value = 商品コード.単価.Value;
    }
}
