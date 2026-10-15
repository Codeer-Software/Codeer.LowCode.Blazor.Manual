//結果が「拒否」「失敗」の行を色で目立たせる。「試行」(操作の前に書く行) は薄く表示する
void ShowResultColor()
{
    if (Result.Value == "Denied")
    {
        Result.Color = "#b91c1c";
        Result.BackgroundColor = "#fee2e2";
    }
    else if (Result.Value == "Failure")
    {
        Result.Color = "#9a3412";
        Result.BackgroundColor = "#ffedd5";
    }
    else if (Result.Value == "Attempt")
    {
        Result.Color = "#6b7280";
    }
}

//一覧の各行
void ListLayout_OnAfterInitialization()
{
    ShowResultColor();
}

//詳細
void DetailLayout_OnAfterInitialization()
{
    ShowResultColor();
}
