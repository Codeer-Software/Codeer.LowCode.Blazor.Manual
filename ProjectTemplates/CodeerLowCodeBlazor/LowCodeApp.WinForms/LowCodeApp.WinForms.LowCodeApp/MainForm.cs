using LowCodeApp.WinForms.LowCodeApp.Services;
using LowCodeApp.WinForms.LowCodeApp.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace LowCodeApp.WinForms.LowCodeApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();

            services.AddSharedServices();

            _blazorWebView.HostPage = "wwwroot\\index.html";
            _blazorWebView.Services = services.BuildServiceProvider();
            _blazorWebView.RootComponents.Add<App>("#app");
            _blazorWebView.RootComponents.Add<HeadOutlet>("head::after");

            Application.ThreadException += Application_ThreadException;
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //�J�����̗�O����
            MessageBox.Show(
                $"An unhandled exception occurred:  {e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}",
                "Error");
            Close();
        }

        private void _developerToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        }
    }
}
