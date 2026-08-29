using ApexCharts;
using Blazor.KHandyInterop;
using Codeer.LowCode.Bindings.ApexCharts.Designer;
using Codeer.LowCode.Bindings.ApexCharts.Designer.Controls;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Designs;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Designer.Extensibility.Views;
using Codeer.LowCode.Blazor.Designer.Models;
using Codeer.LowCode.Blazor.Designer.Views.Windows;
using Codeer.LowCode.Blazor.Extras.Designer;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;
using IgniteUI.Blazor.Controls;
using Codeer.LowCode.Blazor.Extras.ScriptObjects;
using LowCodeSamples.Designer.Lib.ModuleToClass;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Radzen;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace LowCodeSamples.Designer
{
    public partial class App : DesignerApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //load dll.
            typeof(LowCodeSamples.Client.Shared.Services.AppInfoService).ToString();
            typeof(ApexChartFieldDesign).ToString();
            typeof(global::ApexCharts.SeriesType).ToString();
            typeof(FluentTextFieldDesign).ToString();
            typeof(Appearance).ToString();

            MudBlazorLoader.LoadAssemblies();
            RadzenLoader.LoadAssemblies();
            ApexChartsDesignerInitializer.Initialize(BlazorRuntime);
            ExtrasDesignerInitializer.Initialize(BlazorRuntime);

            Services.AddApexCharts();
            PropertyTypeManager.AddPropertyControl<ChartSeries, ChartSeriesPropertyControl>();

            Codeer.LowCode.Blazor.License.LicenseManager.IsAutoUpdate = bool.TryParse(ConfigurationManager.AppSettings["IsLicenseAutoUpdate"], out var val) ? val : true;

            Services.AddSingleton<IDbAccessorFactory, DbAccessorFactory>();
            Services.AddIgniteUIBlazor();
            Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));
            Services.AddMudServices();
            Services.AddRadzenComponents();
            ScriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            ScriptRuntimeTypeManager.AddType(typeof(Codeer.LowCode.Blazor.Extras.ScriptObjects.Excel));
            ScriptRuntimeTypeManager.AddService(new Toaster(null!));
            ScriptRuntimeTypeManager.AddService(new WebApiService(null!, null!));
            ScriptRuntimeTypeManager.AddType<WebApiResult>();
            ScriptRuntimeTypeManager.AddService(new MailService());
            ScriptRuntimeTypeManager.AddType<MailMessage>();
            ScriptRuntimeTypeManager.AddService(new KJS(null!));
            Services.AddFluentUIComponents();

            BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/light/bootstrap.css");
            BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/grid/light/bootstrap.css");
            BlazorRuntime.InstallContentScript("IgniteUI.Blazor", "app.bundle.js");
            BlazorRuntime.InstallBundleCss("LowCodeSamples.Client.Shared");
            BlazorRuntime.InstallBundleCss("Codeer.LowCode.Bindings.Fluent.Blazor");
            BlazorRuntime.InstallAssemblyInitializer(typeof(RadzenTextFieldDesign).Assembly);
            BlazorRuntime.InstallAssemblyInitializer(typeof(MudTextFieldDesign).Assembly);
            BlazorRuntime.InstallRenderProvider(typeof(MudBlazorInstaller));
            BlazorRuntime.InstallAssemblyInitializer(typeof(FluentTextFieldDesign).Assembly);

            IconCandidate.Icons.AddRange(LowCodeSamples.Designer.Properties.Resources.bootstrap_icons.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Order());

            ProjectCatalog.Add(new ProjectCatalogEntry
            {
                Create = CreateGettingStandard,
                Name = "GettingStarted",
                Description = "The sample project reads, writes, and deletes data in the \r\n\"C:\\Codeer.LowCode.Blazor.Local\"; folder. \r\n;Please do not place any data in this folder that would be problematic if overwritten or deleted. You can change this folder later.",
            });
            ProjectCatalog.Add(new ProjectCatalogEntry
            {
                Create = CreateEmpty,
                Name = "Empty",
                Description = "Empty template.",
            });

            base.OnStartup(e);

            MainWindow.Title = "LowCodeSamples";

            //Extras のセットアップメニュー (Tools > 承認フローのセットアップ / メールのセットアップ)
            ExtrasDesignerInitializer.Setup(DesignerEnvironment);
            DesignerEnvironment.AddSolutionExplorerMenu(CreateFieldDataClass, SolutionExplorerMenuTarget.Module, "Create FieldData Class");
        }

        private void CreateFieldDataClass(SolutionExplorerMenuClickEventArgs e)
        {
            var modName = e.Item.Split(".").First();
            var mod = DesignerEnvironment.GetDesignData().Modules.Find(modName);
            if (mod == null)
            {
                DesignerEnvironment.ShowToast("Module not found", false);
                return;
            }

            var classTxt = ClassGenerator.ModuleDesignToDataFieldClass(mod);

            new TextDisplayWindow
            {
                DisplayText = classTxt,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "Module to Field Data Class",
            }.Show();
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
