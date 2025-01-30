void DetailLayoutDesign_OnAfterInitialization()
{
    if(ApproverDetailTileList.RowCount > 0)
    {
        if(ApproverDetailTileList.Rows[0].Approved.Value == true)
        {
            ApproverDetailTileList.IsEnabled = false;
            WorkFlowContent_ViewOnly();
        }
        Approver_TaskWorker_check();
    } 
}

void UserIdLink_OnDataChanged()
{
    if(ApproverDetailTileList.RowCount == 0)
    {
        ApproverDetailTileList.AddRow();
        TaskWorker_Set();
    }
    
    if(ApproverDetailTileList.RowCount > 0)
    {
        TaskWorker_Set();
        Approver_TaskWorker_check();
    }
}

void ApproverDetailTileList_OnDataChanged()
{
    if(ApproverDetailTileList.RowCount > 0)
    {
        Approver_TaskWorker_check();
    }
}

void List_OnDataChanged()
{
    if(WorkFlowDetailList.RowCount > 0)
    {
        Total_Amount_Set();
    }
}

void TaskWorker_Set()
{
    var row = ApproverDetailTileList.Rows[0];
    if(row.Approved.Value != true)
    {
        row.ApproderId_UserMasterLink.Value = UserIdLink.Value;
    }
}

void Approver_TaskWorker_check()
{
    foreach(var i in ApproverDetailTileList.Rows)
    {
        if(i == ApproverDetailTileList.Rows[0])
        {
            i.ApprovedText.Value = "申請";
        }
        else
        {
            i.ApprovedText.Value = "承認";
        }
        
        if(i.ApproderId_UserMasterLink.Value == UserIdLink.Value)
        {
            i.Approved.IsViewOnly = false;
            i.Approved.IsEnabled = true;
        }
        else
        {
            i.Approved.IsViewOnly = true;
        }
    }
}

void Total_Amount_Set()
{
    Amount.Value = 0;
    foreach(var i in WorkFlowDetailList.Rows)
    {
        Amount.Value += i.Amount.Value;
    }
}

void WorkFlowContent_ViewOnly()
{
    WorkFlowName.IsViewOnly = true;
    StartPeriod.IsViewOnly = true;
    EndPeriod.IsViewOnly = true;
    WorkFlowDetailList.IsEnabled = false;
    Remarks.IsViewOnly = true;
}