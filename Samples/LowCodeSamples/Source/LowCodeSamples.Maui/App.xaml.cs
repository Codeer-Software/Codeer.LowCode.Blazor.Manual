namespace LowCodeSamples.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(CreateMainPage());

        //NavigationPage gives the Blazor page a native title bar with the Settings item and hosts SettingsPage.
        public static Page CreateMainPage() => new NavigationPage(new MainPage());
    }
}
