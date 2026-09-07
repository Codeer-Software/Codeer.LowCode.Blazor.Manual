using LowCodeSamples.Maui.Services;

namespace LowCodeSamples.Maui
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            ServerUrlEntry.Text = ServerSettings.BaseUrl;
            DefaultLabel.Text = $"Default (appsettings.json): {ServerSettings.DefaultBaseUrl}";
        }

        void OnSaveClicked(object? sender, EventArgs e)
        {
            var text = ServerUrlEntry.Text?.Trim() ?? string.Empty;
            if (!ServerSettings.IsValidUrl(text))
            {
                ErrorLabel.IsVisible = true;
                return;
            }
            ErrorLabel.IsVisible = false;
            Apply(text.EndsWith('/') ? text : text + "/");
        }

        void OnResetClicked(object? sender, EventArgs e) => Apply(ServerSettings.DefaultBaseUrl);

        //The Blazor side keeps its HttpClient for the lifetime of the WebView, so the app restarts the
        //main page to pick up the new server.
        void Apply(string url)
        {
            ServerSettings.BaseUrl = url;
            if (Window != null) Window.Page = App.CreateMainPage();
        }
    }
}
