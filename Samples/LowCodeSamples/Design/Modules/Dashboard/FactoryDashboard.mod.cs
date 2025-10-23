
void DetailLayoutDesign_OnAfterInitialization()
{
    稼働率.Value = 78.6;
    不良率.Value = 0.9;
    今日の生産数.Value = 4180;
    ライン停止件数.Value = 3;
    
    Meter.Value = 0.9;
    Meter1.Value = 0.2;
    Meter2.Value = 0.6;
    
    using var suspend = SuspendNotifyStateChanged();
    発注データ();
}

void 発注データ()
{
    var orderDate = DateOnly.FromDateTime(DateTime.Now);
    var deliveryDate = orderDate.AddMonths(1);
    
    var row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0001";
    row.品目名.Value = "ワイヤレスマウス";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0002";
    row.品目名.Value = "メカニカルキーボード";
    row.数量.Value = 3;
    row.単価.Value = 7980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0003";
    row.品目名.Value = "USB-C ハブ 7ポート";
    row.数量.Value = 2;
    row.単価.Value = 3480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0004";
    row.品目名.Value = "27インチモニター";
    row.数量.Value = 4;
    row.単価.Value = 23800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0005";
    row.品目名.Value = "ノートPCスタンド";
    row.数量.Value = 6;
    row.単価.Value = 2580;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0006";
    row.品目名.Value = "HDMIケーブル 2m";
    row.数量.Value = 10;
    row.単価.Value = 980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0007";
    row.品目名.Value = "Webカメラ 1080p";
    row.数量.Value = 2;
    row.単価.Value = 4980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0008";
    row.品目名.Value = "ノイズキャンセリングヘッドセット";
    row.数量.Value = 3;
    row.単価.Value = 12800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0009";
    row.品目名.Value = "外付けSSD 1TB";
    row.数量.Value = 4;
    row.単価.Value = 11800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0010";
    row.品目名.Value = "テンキー（有線）";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
}