bool Age_OnValidateInput()
{
    if (Age.Value == null) return true;
    if (Age.Value < 0 || Age.Value > 150)
    {
        Age.SetError("0〜150 の範囲で入力してください。");
        return false;
    }
    Age.ClearError();
    return true;
}

bool Email_OnValidateInput()
{
    if (string.IsNullOrEmpty(Email.Value)) return true;
    if (Email.Value.IndexOf("@") < 0)
    {
        Email.SetError("@ を含むメールアドレスを入力してください。");
        return false;
    }
    Email.ClearError();
    return true;
}
