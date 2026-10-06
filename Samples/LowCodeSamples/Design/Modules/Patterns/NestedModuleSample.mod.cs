void OnAfterInitialization()
{
    if (string.IsNullOrEmpty(ModuleSelector.Value))
    {
        ModuleSelector.Value = "ContactInfo";
    }
}

void ModuleSelector_OnDataChanged()
{
    Contact.SetModule(ModuleSelector.Value, "");
}
