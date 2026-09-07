using Azure.AI.OpenAI;
using Blazor.KHandyInterop;
using Codeer.LowCode.Bindings.ApexCharts.Designer;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Designs;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Designer.Extensibility.Views;
using Codeer.LowCode.Blazor.Designer.Standard;
using Codeer.LowCode.Blazor.Designer.Views.Windows;
using Codeer.LowCode.Blazor.Extras.Designer;
using Codeer.LowCode.Blazor.Script;
using IgniteUI.Blazor.Controls;
using LowCodeSamples.Designer.Lib.ModuleToClass;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using MudBlazor.Services;
using Radzen;
using System.ClientModel;
using System.Configuration;
using System.Windows;

namespace LowCodeSamples.Designer;

public partial class App : DesignerApp
{
    //AZURE_OPENAI_* の3つが揃っているときだけ Azure OpenAI の IChatClient ファクトリを返す(欠けていればAIチャット無効)。
    static Func<IChatClient>? CreateAzureOpenAIChatClientFactory()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_ENDPOINT");
        var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_MODEL");
        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(model)) return null;

        return () => new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key))
            .GetChatClient(model)
            .AsIChatClient();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        //load dll.
        typeof(LowCodeSamples.Client.Shared.Services.AppInfoService).ToString();

        //プロジェクトテンプレートと claude-workspace verb は headless CLI からも参照されるため、
        //headless 分岐が走る base.OnStartup より前に登録する (冪等)。
        DesignerStandard.SetupHeadless();
        ApexChartsDesignerInitializer.Initialize(BlazorRuntime);
        ExtrasDesignerInitializer.Initialize(BlazorRuntime);

        Codeer.LowCode.Blazor.License.LicenseManager.IsAutoUpdate =
            bool.TryParse(ConfigurationManager.AppSettings["IsLicenseAutoUpdate"], out var val) ? val : true;

        Services.AddSingleton<IDbAccessorFactory, DbAccessorFactory>();
        ScriptRuntimeTypeManager.AddService(new LoadingService());
        ScriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();

        BlazorRuntime.InstallBundleCss("LowCodeSamples.Client.Shared");

        //サンプル固有: UI ライブラリのバインディング (Fluent UI / MudBlazor / Radzen / Ignite UI) とハンディターミナル (KHandy)
        typeof(FluentTextFieldDesign).ToString();
        typeof(Appearance).ToString();
        MudBlazorLoader.LoadAssemblies();
        RadzenLoader.LoadAssemblies();
        Services.AddFluentUIComponents();
        Services.AddMudServices();
        Services.AddRadzenComponents();
        Services.AddIgniteUIBlazor();
        Services.AddIgniteUIBlazor(typeof(IgbGridModule), typeof(IgbLegendModule), typeof(IgbCategoryChartModule));
        BlazorRuntime.InstallBundleCss("Codeer.LowCode.Bindings.Fluent.Blazor");
        BlazorRuntime.InstallAssemblyInitializer(typeof(FluentTextFieldDesign).Assembly);
        BlazorRuntime.InstallAssemblyInitializer(typeof(MudTextFieldDesign).Assembly);
        BlazorRuntime.InstallRenderProvider(typeof(MudBlazorInstaller));
        BlazorRuntime.InstallAssemblyInitializer(typeof(RadzenTextFieldDesign).Assembly);
        BlazorRuntime.InstallRenderProvider(typeof(RadzenInstaller));
        BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/light/bootstrap.css");
        BlazorRuntime.InstallContentCss("IgniteUI.Blazor", "themes/grid/light/bootstrap.css");
        BlazorRuntime.InstallContentScript("IgniteUI.Blazor", "app.bundle.js");
        ScriptRuntimeTypeManager.AddService(new KJS(null!));

        base.OnStartup(e);

        MainWindow.Title = "LowCodeSamples";

        //標準実装一式 (アイコン候補 / プロジェクトテンプレート / ツールメニュー / AIチャット) を登録。
        //一部だけ使いたい場合は DesignerStandard.Setup の中身と同じコードを個別に書ける
        //(StandardTemplates / StandardMenus / StandardIcons / DesignerChatRegistration)。
        //AIチャットのモデルはライブラリが IChatClient 抽象しか知らないため、
        //プロバイダ選択(Azure OpenAI)と認証情報はアプリ側のここで持ち、ファクトリとして渡す。
        DesignerStandard.Setup(DesignerEnvironment, new DesignerStandardOptions
        {
            CreateAiChatClient = CreateAzureOpenAIChatClientFactory(),
        });

        //Extras のセットアップメニュー (Tools > 承認フローのセットアップ / メールのセットアップ)
        ExtrasDesignerInitializer.Setup(DesignerEnvironment);

        //サンプル固有: モジュール右クリック → C# の FieldData クラスを生成 (Manual の user_code で使う)
        DesignerEnvironment.AddSolutionExplorerMenu(CreateFieldDataClass, SolutionExplorerMenuTarget.Module, "Create FieldData Class");

        DispatcherUnhandledException += App_DispatcherUnhandledException;
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

    private void App_DispatcherUnhandledException(object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show(
            $"An unhandled exception occurred:  {e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
