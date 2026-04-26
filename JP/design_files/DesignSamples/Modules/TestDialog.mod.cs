
void ボタン_OnClick()
{
    if (string.IsNullOrEmpty(ClassName)) ClassName = "CommandError";
    else ClassName = string.Empty;
}