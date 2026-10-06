void Pref_OnDataChanged()
{
    City.Value = "";
    if (Pref.Value == "東京都")
    {
        City.SetCandidates("千代田区", "新宿区", "渋谷区");
    }
    else if (Pref.Value == "大阪府")
    {
        City.SetCandidates("大阪市", "堺市", "豊中市");
    }
    else if (Pref.Value == "愛知県")
    {
        City.SetCandidates("名古屋市", "豊田市", "岡崎市");
    }
    else
    {
        City.SetCandidates();
    }
}
