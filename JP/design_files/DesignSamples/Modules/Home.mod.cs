
void ボタン_OnClick()
{
    
  //  Toaster.Success(ボタン.HasFocus() +  "," + テキスト.HasFocus());
  //  var dlg = new TestDialog(ModuleLayoutType.Detail);
  //  dlg.ShowDialog();
}
void テキスト_OnDataChanged()
{
    Toaster.Warn("y");
   // Toaster.Success(ボタン.HasFocus() +  "," + テキスト.HasFocus());
}
void GridLayoutDesign_OnKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "Enter") 
    {
        Task.Delay(1);
        Toaster.Success(テキスト.Value);
    }
}