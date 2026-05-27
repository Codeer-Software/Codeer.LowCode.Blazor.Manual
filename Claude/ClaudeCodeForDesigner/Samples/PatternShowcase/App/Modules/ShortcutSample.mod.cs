void OnKeyDown(KeyboardEventArgs e)
{
    if (e.AltKey && e.Key == "s")
    {
        Result.Value = "保存ショートカット (Alt+S) が押されました";
    }
    else if (e.CtrlKey && e.Key == "Enter")
    {
        Result.Value = "送信ショートカット (Ctrl+Enter) が押されました: " + Text.Value;
    }
    else if (e.Key == "Escape")
    {
        Text.Value = "";
        Result.Value = "クリアしました";
    }
}
