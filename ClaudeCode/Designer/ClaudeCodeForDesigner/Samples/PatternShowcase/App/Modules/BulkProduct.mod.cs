void DeleteSelected_OnClick()
{
    foreach (var row in Items.Rows)
    {
        if (row.IsSelected.Value == true)
        {
            Items.DeleteRow(row);
        }
    }
}
