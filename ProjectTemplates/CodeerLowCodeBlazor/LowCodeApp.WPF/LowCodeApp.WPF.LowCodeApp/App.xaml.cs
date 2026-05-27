using System.IO;
using System.Windows;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using Excel.Report.PDF;
using LowCodeApp.WPF.Client.Shared.Samples;
using LowCodeApp.WPF.LowCodeApp.Services;
using LowCodeApp.WPF.LowCodeApp.Services.FileManagement;
using Microsoft.Extensions.Configuration;
using PdfSharp.Fonts;

namespace LowCodeApp.WPF.LowCodeApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //load dll.
            typeof(CodeBehindSample).ToString();

            GlobalFontSettings.FontResolver = new CustomFontResolver();
            LowCodeApp.WPF.Client.Shared.ScriptObjects.Excel.ConvertPdf = e => ExcelConverter.ConvertToPdf(e, 1);
            LowCodeApp.WPF.Client.Shared.ScriptObjects.MailService.SendEmailAsyncCore = MailService.SendEmailAsync;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
            LicenseManager.DomainLicense = config.GetSection("DomainLicense").Get<string>() ?? string.Empty;
            LicenseManager.IsAutoUpdate = config.GetSection("IsLicenseAutoUpdate").Get<bool>();
            SystemConfig.Instance.UseHotReload = config.GetSection("UseHotReload").Get<bool>();
            SystemConfig.Instance.DataSources = config.GetSection("DataSources").Get<DataSource[]>() ?? new DataSource[0];
            SystemConfig.Instance.FileStorages = config.GetSection("FileStorages").Get<FileStorage[]>() ?? new FileStorage[0];
            SystemConfig.Instance.MailSettings = config.GetSection("MailSettings").Get<MailSettings>() ?? new();
            SystemConfig.Instance.AISettings = config.GetSection("AISettings").Get<AISettings>() ?? new();
            SystemConfig.Instance.TemporaryFileTableInfo = config.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? new TemporaryFileTableInfo[0];
            SystemConfig.Instance.DesignFileDirectory = config["DesignFileDirectory"] ?? string.Empty;
            SystemConfig.Instance.FontFileDirectory = config["FontFileDirectory"] ?? string.Empty;

            foreach (var dataSource in SystemConfig.Instance.DataSources)
            {
                dataSource.ConnectionString = config.GetConnectionString(dataSource.Name) ?? string.Empty;
            }

            foreach (var fileStorage in SystemConfig.Instance.FileStorages)
            {
                fileStorage.ConnectionString = config.GetConnectionString(fileStorage.Name) ?? string.Empty;
            }

            using (var httpClient = new HttpClient(new WinHttpHandler { WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseWinInetProxy }))
            {
                var thread = new Thread(() => LicenseManager.CheckClientServerLicense(httpClient).Wait());
                thread.Start();
                thread.Join();
            }

            base.OnStartup(e);
        }
    }
}
