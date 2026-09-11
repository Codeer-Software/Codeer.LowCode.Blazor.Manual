namespace LowCodeNativeSamples.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void OnSettingsClicked(object? sender, EventArgs e)
            => await Navigation.PushAsync(new SettingsPage());

        //Recreates the BlazorWebView so the design files are fetched from the server again
        //(the design is loaded once per WebView; use this after deploying a new design).
        void OnReloadClicked(object? sender, EventArgs e)
        {
            if (Window != null) Window.Page = App.CreateMainPage();
        }
    }
}
