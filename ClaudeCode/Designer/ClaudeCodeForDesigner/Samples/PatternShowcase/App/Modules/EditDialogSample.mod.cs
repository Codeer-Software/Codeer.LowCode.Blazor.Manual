void AddCommentButton_OnClick()
{
    var dlg = new EditDialogTarget(ModuleLayoutType.Detail);
    if (dlg.ShowDialog("投稿", "キャンセル") == "投稿")
    {
        Comments.AddRow(dlg);
    }
}
