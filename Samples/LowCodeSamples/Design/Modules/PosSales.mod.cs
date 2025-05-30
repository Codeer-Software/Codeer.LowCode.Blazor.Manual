void DetailLayoutDesign_OnBeforeInitialization()
{
    listProduct.AllowLoad = false;
}

void DetailLayoutDesign_OnAfterInitialization()
{
    numOutQty.Value=1;
    txtBarcodeInput.Focus();
    
}

void txtBarcodeInput_OnDataChanged()
{
     
    if(string.IsNullOrEmpty(txtBarcodeInput.Value.Trim())) return;
    
    listProduct.AllowLoad = true;
    
    txtCode.Value = txtBarcodeInput.Value;
    var searcher = new ModuleSearcher<PosProducts>();
    var targetCode = txtBarcodeInput.Value;
    searcher.AddEquals(e => e.Code.Value, targetCode);
    
    listProduct.SetAdditionalConditionAsync(searcher);
    listProduct.Reload();
    
    txtBarcodeInput.Value=string.Empty;
    
}


void btnClear_OnClick()
{
    txtBarcodeInput.Value=string.Empty;
    txtBarcodeInput.Focus();
}
