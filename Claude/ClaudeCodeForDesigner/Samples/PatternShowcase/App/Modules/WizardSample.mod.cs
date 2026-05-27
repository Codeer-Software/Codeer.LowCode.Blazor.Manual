void OnInit()
{
    Step.Value = 1;
    UpdateStepVisibility();
}

void UpdateStepVisibility()
{
    Step1Section.IsVisible = Step.Value == 1;
    Name.IsVisible = Step.Value == 1;
    Step2Section.IsVisible = Step.Value == 2;
    Email.IsVisible = Step.Value == 2;
    Step3Section.IsVisible = Step.Value == 3;
    Confirm.IsVisible = Step.Value == 3;
    PrevStep.IsVisible = Step.Value > 1;
    NextStep.IsVisible = Step.Value < 3;
    Finish.IsVisible = Step.Value == 3;
}

void NextStep_OnClick()
{
    Step.Value++;
    if (Step.Value == 3)
    {
        Confirm.Value = "名前: " + Name.Value + " / メール: " + Email.Value;
    }
    UpdateStepVisibility();
}

void PrevStep_OnClick()
{
    Step.Value--;
    UpdateStepVisibility();
}

void Finish_OnClick()
{
    MessageBox.Show("登録が完了しました。", "OK");
}
