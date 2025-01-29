
void Approved_OnDataChanged()
{
    ApprovalDate_Set();
}

void ApprovalDate_Set()
{
    if(Approved.Value == true)
    {
        ApprovalDate.Value = DateOnly.FromDateTime((DateTime.Now));
    }
    else
    {
        ApprovalDate.Value = null;
    }
}