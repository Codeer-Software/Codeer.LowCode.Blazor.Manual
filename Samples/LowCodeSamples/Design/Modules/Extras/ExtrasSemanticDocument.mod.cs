//デモサイトはモジュールが更新不可 (表示専用) なので、ボタンも押せない状態になる。索引付けのボタンだけ押せるようにする
void DetailLayoutDesign_OnAfterInitialization()
{
    ReindexButton.IsViewOnly = false;
}

//デモデータを SQL で入れた後に 1 回押す: 索引 (検索用の文章と埋め込み) の無い文書だけに付ける。
//文章の無い行は行から作る (添付の中身も取り出して入る)。索引のある文書は何もしない
void ReindexButton_OnClick()
{
    Search.ReindexMissing();
}

//索引付けが終わったとき (成功・失敗・中断のどれでも呼ばれる)
void Search_OnReindexCompleted()
{
    if (Search.ReindexError != "") MessageBox.Show(Search.ReindexError);
    else MessageBox.Show($"{Search.ReindexProcessed} 件の文書に索引を付けました");
}
