
void DetailLayoutDesign_OnAfterInitialization()
{
    車1価格.Value = 2500000;
    車2価格.Value = 3600000;
}

void 車1外装カスタマイズブール_OnDataChanged()
{
    車1カスタマイズCheck();
}

void 車1内装カスタマイズブール_OnDataChanged()
{
    車1カスタマイズCheck();
}

void 車1安全運転支援パックブール_OnDataChanged()
{
    車1カスタマイズCheck();
}

void 車2外装カスタマイズブール_OnDataChanged()
{
    車2カスタマイズCheck();
}

void 車2内装カスタマイズブール_OnDataChanged()
{
    車2カスタマイズCheck();
}

void 車2安全運転支援パックブール_OnDataChanged()
{
    車2カスタマイズCheck();
}

void 車1カスタマイズCheck()
{
    車1価格初期化();
    if(車1外装カスタマイズブール.Value)
    {
        車1価格.Value += 2500000;
    }
    
    if(車1内装カスタマイズブール.Value)
    {
        車1価格.Value += 130000;
    }
    
    if(車1安全運転支援パックブール.Value)
    {
        車1価格.Value += 100000;
    }
}

void 車2カスタマイズCheck()
{
    車2価格初期化();
    if(車2外装カスタマイズブール.Value)
    {
        車2価格.Value += 3500000;
    }
    
    if(車2内装カスタマイズブール.Value)
    {
        車2価格.Value += 230000;
    }
    
    if(車2安全運転支援パックブール.Value)
    {
        車2価格.Value += 150000;
    }
}

void 車1価格初期化()
{
    車1価格.Value = 2500000;
}

void 車2価格初期化()
{
    車2価格.Value = 3600000;
}

void 画像表示車1_OnClick()
{
    車種.Value = 車1ラベル.Text;
    価格.Value = 車1価格.Value;
}
void 画像表示車2_OnClick()
{
    車種.Value = 車2ラベル.Text;
    価格.Value = 車2価格.Value;
}