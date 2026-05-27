using System.Windows;
using LowCodeApp.WPF.LowCodeApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LowCodeApp.WPF.LowCodeApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddSharedServices();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}
