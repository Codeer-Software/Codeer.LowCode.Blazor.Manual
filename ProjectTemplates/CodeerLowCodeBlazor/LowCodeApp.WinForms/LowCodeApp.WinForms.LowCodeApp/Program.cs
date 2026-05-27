using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using Excel.Report.PDF;
using LowCodeApp.WinForms.Client.Shared.Samples;
using LowCodeApp.WinForms.LowCodeApp.Services;
using LowCodeApp.WinForms.LowCodeApp.Services.FileManagement;
using Microsoft.Extensions.Configuration;
using PdfSharp.Fonts;

namespace LowCodeApp.WinForms.LowCodeApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            //load dll.
            typeof(CodeBehindSample).ToString();

            GlobalFontSettings.FontResolver = new CustomFontResolver();
            LowCodeApp.WinForms.Client.Shared.ScriptObjects.Excel.ConvertPdf = e => ExcelConverter.ConvertToPdf(e, 1);
            LowCodeApp.WinForms.Client.Shared.ScriptObjects.MailService.SendEmailAsyncCore = MailService.SendEmailAsync;

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

            Application.Run(new MainForm());
        }
    }
}
