
void DetailLayoutDesign_OnAfterInitialization()
{
    if(IsNewData)
    {
        進捗率.Value = 0;
    }
}

// ステータス と 進捗率 を連動させる
void ステータス_OnDataChanged()
{
    if (ステータス.Value == "完了")
    {
        進捗率.Value = 100;
    }
    else if (ステータス.Value == "未着手")
    {
        進捗率.Value = 0;
    }
    else if (ステータス.Value == "進行中")
    {
        if (進捗率.Value < 1)
        {
            進捗率.Value = 1;
        }
        else if (80 < 進捗率.Value)
        {
            進捗率.Value = 80;
        }
    }
    else if (ステータス.Value == "レビュー")
    {
        if (進捗率.Value < 81)
        {
            進捗率.Value = 81;
        }
        else if (99 < 進捗率.Value)
        {
            進捗率.Value = 99;
        }
    }
}

void 進捗率_OnDataChanged()
{
    if (進捗率.Value == 0 || string.IsNullOrEmpty(進捗率.Value))
    {
        ステータス.Value = "未着手";
    }
    else if (進捗率.Value == 100)
    {
        ステータス.Value = "完了";
    }
    else if (80 < 進捗率.Value)
    {
        ステータス.Value = "レビュー";
    }
    else
    {
        ステータス.Value = "進行中";
    }
}

// 開始日・終了日の前後関係バリデーション（OnValidateInput）
bool 開始日_OnValidateInput()
{
    if (開始日.Value != null && 終了日.Value != null && 終了日.Value < 開始日.Value)
    {
        開始日.SetError("開始日は終了日以前を指定してください");
        return false;
    }
    return true;
}

bool 終了日_OnValidateInput()
{
    if (開始日.Value != null && 終了日.Value != null && 終了日.Value < 開始日.Value)
    {
        終了日.SetError("終了日は開始日以降を指定してください");
        return false;
    }
    return true;
}

void DetailLayoutDesign_OnFieldDataChanged(string fieldName)
{
    var parent = (プロジェクト)GetParentModule();
    if(IsNewData) return;
    if(fieldName == "進捗率") return;
    parent.ステータス別遅延チャート.Reload();
    parent.状態構成比チャート.Reload();
    parent.担当者別遅延チャート.Reload();
}