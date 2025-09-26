
void LED_OnClick()
{
    KJS.StartLedAsync(1, 1000, 1000, 3);
}
void Vibe_OnClick()
{
    KJS.StartVibratorAsync(1000, 1000, 3);
}
void Buzzer_OnClick()
{
    KJS.StartBuzzerAsync(1, 100, 100, 2);
}