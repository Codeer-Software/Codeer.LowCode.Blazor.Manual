
void DetailLayoutDesign_OnAfterInitialization()
{
    図面番号.Value = "FGR-0000001";
    物件名.Value = "ABC株式会社オフィス";
    工区階.Value = 2;
}

/*ドアMarker*/
void 部屋1ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋1情報();
}
void 部屋2ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋2情報();
}
void 部屋3ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋3情報();
}
void 部屋4ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋4情報();
}
void 部屋5ドアMarker_OnClick(int x, int y)
{
    ドア情報2();
    部屋5情報();
}
void 部屋6ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋6情報();
}
void 部屋7ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋7情報();
}
void 部屋8ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋8情報();
}
void 部屋9ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋9情報();
}
void 部屋10ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋10情報();
}
void 部屋11ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋11情報();
}
void 部屋12ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋12情報();
}
void 部屋13ドアMarker_OnClick(int x, int y)
{
    ドア情報();
    部屋13情報();
}

/*窓Marker*/
void 部屋1窓Marker_OnClick(int x, int y)
{
    部屋1情報();
    窓情報();
}
void 部屋2窓Marker_OnClick(int x, int y)
{
    部屋2情報();
    窓情報2();
}
void 部屋3窓Marker_OnClick(int x, int y)
{
    部屋3情報();
    窓情報();
}
void 部屋4窓Marker_OnClick(int x, int y)
{
    部屋4情報();
    窓情報();
}
void 部屋4窓Marker11_OnClick(int x, int y)
{
    部屋4情報();
    窓情報2();
}
void 部屋7窓Marker_OnClick(int x, int y)
{
    部屋7情報();
    窓情報();
}
void 部屋8窓Marker_OnClick(int x, int y)
{
    部屋8情報();
    窓情報2();
}
void 部屋10窓Marker_OnClick(int x, int y)
{
    部屋10情報();
    窓情報();
}
void 部屋11窓Marker_OnClick(int x, int y)
{
    部屋11情報();
    窓情報();
}
void 部屋12窓Marker_OnClick(int x, int y)
{
    部屋12情報();
    窓情報();
}
void 部屋13窓Marker_OnClick(int x, int y)
{
    部屋13情報();
    窓情報();
}

/*部屋情報*/
void 部屋1情報()
{
    部屋番号.Value = 1;
    用途.Value = "共用";
    面積.Value = 11.1;
    照明数.Value = 1;
}
void 部屋2情報()
{
    部屋番号.Value = 2;
    用途.Value = "共用";
    面積.Value = 2.0;
    照明数.Value = 1;
}
void 部屋3情報()
{
    部屋番号.Value = 3;
    用途.Value = "会議室";
    面積.Value = 6.5;
    照明数.Value = 1;
}
void 部屋4情報()
{
    部屋番号.Value = 4;
    用途.Value = "ワークエリア";
    面積.Value = 7.6;
    照明数.Value = 2;
}
void 部屋5情報()
{
    部屋番号.Value = 5;
    用途.Value = "会議室";
    面積.Value = 7.5;
    照明数.Value = 1;
}
void 部屋6情報()
{
    部屋番号.Value = 6;
    用途.Value = "受付";
    面積.Value = 4.5;
    照明数.Value = 1;
}
void 部屋7情報()
{
    部屋番号.Value = 7;
    用途.Value = "会議室";
    面積.Value = 8.5;
    照明数.Value = 1;
}
void 部屋8情報()
{
    部屋番号.Value = 8;
    用途.Value = "会議室";
    面積.Value = 10.1;
    照明数.Value = 1;
}
void 部屋9情報()
{
    部屋番号.Value = 9;
    用途.Value = "IT";
    面積.Value = 6.0;
    照明数.Value = 1;
}
void 部屋10情報()
{
    部屋番号.Value = 10;
    用途.Value = "会議室";
    面積.Value = 10.6;
    照明数.Value = 1;
}
void 部屋11情報()
{
    部屋番号.Value = 11;
    用途.Value = "会議室";
    面積.Value = 12.0;
    照明数.Value = 1;
}
void 部屋12情報()
{
    部屋番号.Value = 12;
    用途.Value = "会議室";
    面積.Value = 11.2;
    照明数.Value = 1;
}
void 部屋13情報()
{
    部屋番号.Value = 13;
    用途.Value = "ワークエリア";
    面積.Value = 8.0;
    照明数.Value = 2;
}

/*ドア・窓情報*/
void ドア情報()
{
    種別.Value = "ドア";
    サイズ.Value = "800,2000,33,730";
    方式ラベル.Text = "片開き";
}

void ドア情報2()
{
    種別.Value = "ドア";
    サイズ.Value = "1400,2200,40,630";
    方式ラベル.Text = "両開き";
}

void 窓情報()
{
    種別.Value = "窓";
    サイズ.Value = "1650,1170";
    方式ラベル.Text = "開閉式";
}

void 窓情報2()
{
    種別.Value = "窓";
    サイズ.Value = "850,1000";
    方式ラベル.Text = "上下窓";
}

void 保存ボタン_OnClick()
{
    Toaster.Warn("DBと接続してないので保存できません");
}