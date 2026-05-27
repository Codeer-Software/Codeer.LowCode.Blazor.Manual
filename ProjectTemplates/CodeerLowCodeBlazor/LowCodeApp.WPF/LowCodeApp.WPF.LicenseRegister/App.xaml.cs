using System.Windows;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Views.Windows;

namespace LowCodeApp.WPF.LicenseRegister
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : DesignerApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterLicense.Open();
            Shutdown(0);
        }
    }

}
