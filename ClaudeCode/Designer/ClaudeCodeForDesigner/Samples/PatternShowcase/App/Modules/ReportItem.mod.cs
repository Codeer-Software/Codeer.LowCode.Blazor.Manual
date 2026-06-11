void Quantity_OnDataChanged()
{
    Subtotal.Value = Quantity.Value * UnitPrice.Value;
}

void UnitPrice_OnDataChanged()
{
    Subtotal.Value = Quantity.Value * UnitPrice.Value;
}
