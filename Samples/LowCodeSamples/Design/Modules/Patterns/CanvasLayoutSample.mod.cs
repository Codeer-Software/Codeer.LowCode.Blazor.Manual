void OnAfterInitialization()
{
    if (RoomACount.Value != null) return;

    RoomACount.Value = 4;
    RoomAOwner.Value = "山田";
    RoomBCount.Value = 0;
    RoomBOwner.Value = "";
    RoomCCount.Value = 6;
    RoomCOwner.Value = "佐藤";
    RoomDCount.Value = 0;
    RoomDOwner.Value = "";
    CafeCount.Value = 12;
    CafeOwner.Value = "鈴木";
}
