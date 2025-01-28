using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Models;
using Codeer.LowCode.Blazor.Script;
using LowCodeSamples.Client.Shared.ScriptObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Windows;
using Codeer.LowCode.Bindings.MudBlazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Designs;
using IgniteUI.Blazor.Controls;
using MudBlazor.Services;
using Radzen;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using LowCodeSamples.Client.Shared.AITextAnalyzer;
using Codeer.LowCode.Blazor.Repository.Data;
using System.Net.Http;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;

namespace LowCodeSamples.Designer
{
    public partial class App : DesignerApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            typeof(ApexChartFieldDesign).ToString();
            typeof(SeriesType).ToString();

            MudBlazorLoader.LoadAssemblies();
            RadzenLoader.LoadAssemblies();

            Codeer.LowCode.Blazor.License.LicenseManager.IsAutoUpdate = bool.TryParse(ConfigurationManager.AppSettings["IsLicenseAutoUpdate"], out var val) ? val : true;

            Services.AddSingleton<IDbAccessorFactory, DbAccessorFactory>();
            Services.AddSingleton<IAITextAnalyzerCore, AITextAnalyzerCoreDummy>();
            Services.AddIgniteUIBlazor();
            Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));
            Services.AddMudServices();
            Services.AddRadzenComponents();
            ScriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            ScriptRuntimeTypeManager.AddType(typeof(LowCodeSamples.Client.Shared.ScriptObjects.Excel));
            ScriptRuntimeTypeManager.AddService(new Toaster(null!));
            ScriptRuntimeTypeManager.AddService(new WebApiService(null!, null!));
            ScriptRuntimeTypeManager.AddType<WebApiResult>();

            BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/light/bootstrap.css");
            BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/grid/light/bootstrap.css");
            BlazorRuntime.InstallContentScript("IgniteUI.Blazor", "app.bundle.js");
            BlazorRuntime.InstallBundleCss("LowCodeSamples.Client.Shared");
            BlazorRuntime.InstallAssemblyInitializer(typeof(RadzenTextFieldDesign).Assembly);
            BlazorRuntime.InstallAssemblyInitializer(typeof(MudTextFieldDesign).Assembly);
            BlazorRuntime.InstallRenderProvider(typeof(MudBlazorInstaller));

            IconCandidate.Icons.AddRange(LowCodeSamples.Designer.Properties.Resources.bootstrap_icons.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Order());

            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateGettingStandard,
                Name = "GettingStarted",
                Description = "The sample project reads, writes, and deletes data in the \r\n\"C:\\Codeer.LowCode.Blazor.Local\"; folder. \r\n;Please do not place any data in this folder that would be problematic if overwritten or deleted. You can change this folder later.",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateEmpty,
                Name = "Empty",
                Description = "Empty template.",
            });

            base.OnStartup(e);

            MainWindow.Title = "LowCodeSamples";
        }

        class AITextAnalyzerCoreDummy : IAITextAnalyzerCore
        {
            public Task<ModuleData?> FileToModuleDataAsync(string moduleName, string fileName, StreamContent content)
                => throw new NotImplementedException();

            public Task<ModuleData?> TextToModuleDataAsync(string moduleName, string text)
                => throw new NotImplementedException();
        }

        static void CreateEmpty(string path)
        {
            using (Stream stream = new MemoryStream(LowCodeSamples.Designer.Properties.Resources.EmptyTemplate))
            {
                ZipFile.ExtractToDirectory(stream, path);
            }
        }

        static void CreateGettingStandard(string path)
        {
            using (Stream stream = new MemoryStream(LowCodeSamples.Designer.Properties.Resources.GettingStartedTemplate))
            {
                ZipFile.ExtractToDirectory(stream, path);
            }

            var dbPath = "C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample.db";
            if (!File.Exists(dbPath))
            {
                if (!File.Exists(dbPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                    File.WriteAllBytes(dbPath, LowCodeSamples.Designer.Properties.Resources.sqlite_sample);
                }
            }
        }
    }
}
