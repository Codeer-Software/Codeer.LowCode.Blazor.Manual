using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Models;
using Codeer.LowCode.Blazor.Script;
using IgniteUI.Blazor.Controls;
using IGSample.Client.Shared.ScriptObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace IGSample.Designer
{
  public partial class App : DesignerApp
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      Codeer.LowCode.Blazor.License.LicenseManager.IsAutoUpdate = bool.TryParse(ConfigurationManager.AppSettings["IsLicenseAutoUpdate"], out var val) ? val : true;

      Services.AddSingleton<IDbAccessorFactory, DbAccessorFactory>();
      Services.AddIgniteUIBlazor();
      Services.AddIgniteUIBlazor(typeof(IgbGridModule));
      ScriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
      ScriptRuntimeTypeManager.AddType(typeof(IGSample.Client.Shared.ScriptObjects.Excel));
      ScriptRuntimeTypeManager.AddService(new Toaster(null!));
      ScriptRuntimeTypeManager.AddService(new WebApiService(null!, null!));
      ScriptRuntimeTypeManager.AddType<WebApiResult>();

      InstallContentCss("IgniteUI.Blazor", "themes/light/bootstrap.css");
      InstallContentCss("IgniteUI.Blazor", "themes/grid/light/bootstrap.css");
      InstallContentScript("IgniteUI.Blazor", "app.bundle.js");
      InstallContentCss("FrappeGanttJS.Blazor", "frappe-gantt.min.css");
      InstallContentScript("FrappeGanttJS.Blazor", "frappe-gantt.min.js");
      InstallContentScript("FrappeGanttJS.Blazor", "main.js");
      InstallBundleCss("IGSample.Client.Shared");
      InstallBundleCss("FrappeGanttJS.Blazor");

      IconCandidate.Icons.AddRange(IGSample.Designer.Properties.Resources.bootstrap_icons.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Order());

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

      MainWindow.Title = "IGSample";
    }

    static void CreateEmpty(string path)
    {
      using (Stream stream = new MemoryStream(IGSample.Designer.Properties.Resources.EmptyTemplate))
      {
        ZipFile.ExtractToDirectory(stream, path);
      }
    }

    static void CreateGettingStandard(string path)
    {
      using (Stream stream = new MemoryStream(IGSample.Designer.Properties.Resources.GettingStartedTemplate))
      {
        ZipFile.ExtractToDirectory(stream, path);
      }

      var dbPath = "C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample.db";
      if (!File.Exists(dbPath))
      {
        if (!File.Exists(dbPath))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
          File.WriteAllBytes(dbPath, IGSample.Designer.Properties.Resources.sqlite_sample);
        }
      }
    }
  }
}
