void DetailLayoutDesign_OnAfterInitialization()
{
    進捗.Value = 65;

    AddTask("要件定義", 100, "#2e7d32");
    AddTask("設計", 80, "#1a73e8");
    AddTask("実装", 55, "");
    AddTask("テスト", 20, "#e37400");
    AddTask("リリース準備", 0, "#9e9e9e");
}

void AddTask(string name, decimal progress, string color)
{
    var row = タスクリスト.AddRow();
    row.タスク名.Value = name;
    row.進捗.Value = progress;
    row.バー色.Value = color;
}
