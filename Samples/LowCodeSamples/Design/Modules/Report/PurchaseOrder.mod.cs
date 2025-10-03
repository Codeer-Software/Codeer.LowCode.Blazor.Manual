
void DetailLayoutDesign_OnAfterInitialization()
{
    using var suspend = SuspendNotifyStateChanged();
    var orderDate = DateOnly.FromDateTime(DateTime.Now);
    var deliveryDate = orderDate.AddMonths(1);
    合計金額.Value = 0;
    
    var row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0001";
    row.品目名.Value = "ワイヤレスマウス";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0002";
    row.品目名.Value = "メカニカルキーボード";
    row.数量.Value = 3;
    row.単価.Value = 7980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0003";
    row.品目名.Value = "USB-C ハブ 7ポート";
    row.数量.Value = 2;
    row.単価.Value = 3480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0004";
    row.品目名.Value = "27インチモニター";
    row.数量.Value = 4;
    row.単価.Value = 23800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0005";
    row.品目名.Value = "ノートPCスタンド";
    row.数量.Value = 6;
    row.単価.Value = 2580;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0006";
    row.品目名.Value = "HDMIケーブル 2m";
    row.数量.Value = 10;
    row.単価.Value = 980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0007";
    row.品目名.Value = "Webカメラ 1080p";
    row.数量.Value = 2;
    row.単価.Value = 4980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0008";
    row.品目名.Value = "ノイズキャンセリングヘッドセット";
    row.数量.Value = 3;
    row.単価.Value = 12800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0009";
    row.品目名.Value = "外付けSSD 1TB";
    row.数量.Value = 4;
    row.単価.Value = 11800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0010";
    row.品目名.Value = "テンキー（有線）";
    row.数量.Value = 5;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0011";
    row.品目名.Value = "レーザープリンタ用トナー";
    row.数量.Value = 2;
    row.単価.Value = 6480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0012";
    row.品目名.Value = "A4 コピー用紙（500枚）";
    row.数量.Value = 8;
    row.単価.Value = 780;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0013";
    row.品目名.Value = "USBメモリ 64GB";
    row.数量.Value = 12;
    row.単価.Value = 980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0014";
    row.品目名.Value = "LANケーブル Cat6 5m";
    row.数量.Value = 7;
    row.単価.Value = 680;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0015";
    row.品目名.Value = "Bluetooth スピーカー";
    row.数量.Value = 3;
    row.単価.Value = 3980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0016";
    row.品目名.Value = "ACアダプタ 65W";
    row.数量.Value = 2;
    row.単価.Value = 3480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0017";
    row.品目名.Value = "スマホ用三脚";
    row.数量.Value = 6;
    row.単価.Value = 1580;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0018";
    row.品目名.Value = "オフィスチェアクッション";
    row.数量.Value = 5;
    row.単価.Value = 2480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0019";
    row.品目名.Value = "USB-C → HDMI 変換アダプタ";
    row.数量.Value = 4;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0020";
    row.品目名.Value = "モニターアーム シングル";
    row.数量.Value = 2;
    row.単価.Value = 5980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
    
    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0021";
    row.品目名.Value = "ケーブルタイ（100本）";
    row.数量.Value = 5;
    row.単価.Value = 480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0022";
    row.品目名.Value = "マウスパッド";
    row.数量.Value = 7;
    row.単価.Value = 780;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0023";
    row.品目名.Value = "クリーニングクロス";
    row.数量.Value = 15;
    row.単価.Value = 320;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0024";
    row.品目名.Value = "スマートプラグ";
    row.数量.Value = 4;
    row.単価.Value = 1980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0025";
    row.品目名.Value = "ドキュメントスキャナ";
    row.数量.Value = 1;
    row.単価.Value = 25800;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0026";
    row.品目名.Value = "USB-C 充電器 30W";
    row.数量.Value = 3;
    row.単価.Value = 2480;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0027";
    row.品目名.Value = "LEDデスクライト";
    row.数量.Value = 2;
    row.単価.Value = 3980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0028";
    row.品目名.Value = "耐震ジェルパッド";
    row.数量.Value = 12;
    row.単価.Value = 980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0029";
    row.品目名.Value = "ケーブルオーガナイザー";
    row.数量.Value = 6;
    row.単価.Value = 1280;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;

    row = 発注書明細リスト.AddRow();
    row.品目コード.Value = "ITM0030";
    row.品目名.Value = "ラベルライター";
    row.数量.Value = 2;
    row.単価.Value = 6980;
    row.発注日.Value = orderDate;
    row.納期.Value = deliveryDate;
    合計金額.Value += row.合計.Value;
}

void PDFボタン_OnClick()
{
    出力日.Value = DateTime.Now;
    using(var memory = Resources.GetMemoryStream("PurchaseOrder.xlsx"))
    {
        var excel = new Excel(memory, "PurchaseOrder");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}

void Excelテンプレートダウンロードボタン_OnClick()
{
    using(var memory = Resources.GetMemoryStream("PurchaseOrder.xlsx"))
    {
        var excel = new Excel(memory, "PurchaseOrder");
        excel.Download();
    }
}