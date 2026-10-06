void Recalc()
{
    var u = UnitPrice.Value == null ? 0 : UnitPrice.Value;
    var q = Quantity.Value == null ? 0 : Quantity.Value;
    Total.Value = u * q;
}
