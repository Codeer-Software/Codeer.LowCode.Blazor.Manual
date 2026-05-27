using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using Codeer.LowCode.Bindings.ApexCharts.Designer;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.DataIO.Db.Definition;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Designer.Extensibility.Views;
using Codeer.LowCode.Blazor.Designer.Models;
using Codeer.LowCode.Blazor.Designer.Views.Windows;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Designer;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeApp.WinForms.Client.Shared.AITextAnalyzer;
using LowCodeApp.WinForms.Client.Shared.ScriptObjects;
using LowCodeApp.WinForms.Designer.Lib;
using LowCodeApp.WinForms.Designer.Lib.AI;
using LowCodeApp.WinForms.Designer.Lib.DbTableToModule;
using LowCodeApp.WinForms.Designer.Lib.ExcelToModule;
using LowCodeApp.WinForms.Designer.Lib.ModuleToClass;
using LowCodeApp.WinForms.Designer.Lib.SeleniumPageObject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace LowCodeApp.WinForms.Designer
{
    public partial class App : DesignerApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ApexChartsDesignerInitializer.Initialize(BlazorRuntime);
            ExtrasDesignerInitializer.Initialize(BlazorRuntime);

            AISettings.Instance.OpenAIEndPoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_ENDPOINT") ?? string.Empty;
            AISettings.Instance.OpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
            AISettings.Instance.ChatModel = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_MODEL") ?? string.Empty;

            Codeer.LowCode.Blazor.License.LicenseManager.IsAutoUpdate =
                bool.TryParse(ConfigurationManager.AppSettings["IsLicenseAutoUpdate"], out var val) ? val : true;
            Services.AddSingleton<IDbAccessorFactory, DbAccessorFactory>();
            Services.AddSingleton<IAITextAnalyzerCore, AITextAnalyzerCoreDummy>();
            ScriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            ScriptRuntimeTypeManager.AddType(typeof(LowCodeApp.WinForms.Client.Shared.ScriptObjects.Excel));
            ScriptRuntimeTypeManager.AddService(new Toaster(null!));
            ScriptRuntimeTypeManager.AddService(new WebApiService(null!, null!));
            ScriptRuntimeTypeManager.AddType<WebApiResult>();
            ScriptRuntimeTypeManager.AddService(new MailService());
            ScriptRuntimeTypeManager.AddService(new LoadingService());
            ScriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();

            BlazorRuntime.InstallBundleCss("LowCodeApp.WinForms.Client.Shared");

            IconCandidate.Icons.AddRange(LowCodeApp.WinForms.Designer.Properties.Resources.bootstrap_icons
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Order());

            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateEmpty,
                Name = "空のプロジェクト",
                Description = "最小構成の空プロジェクト。モジュール / ページフレームを 1 から作りたいときに。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateEmptyAuth,
                Name = "空のプロジェクト（認証付き）",
                Description = "Cookie 認証付きの空プロジェクト。AppUser モジュールとログインまわりの最小構成が組み込み済。初期ユーザーは admin/admin。\n※Visual Studio で新規ソリューションを作成するときは「Codeer.LowCode.Blazor.Cookie」で作成してください。その他のものとは整合しません。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateGettingStandard,
                Name = "入門サンプル",
                Description = "Codeer.LowCode.Blazor を初めて触る方向けの入門用サンプル。著者管理・書籍登録などの最小限の業務画面を一通り含み、デザイナの基本操作を覚えるのに使えます。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreatePatternShowcase,
                Name = "標準パターン集",
                Description = "Codeer.LowCode.Blazor で実現できる標準パターンを集めたサンプル集 (データ操作 / 検索 / リスト / 一覧 / ダイアログ / レイアウト / 入力UX / 出力 / 別フレーム など 50 種以上)。各機能の実装例として参考にしてください。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreatePatternShowcaseAuth,
                Name = "認証パターン集",
                Description = "Cookie 認証を組み込んだ認証・権限パターンのサンプル集 (ユーザー管理 / マイプロフィール / アプリ権限 / PageFrame 権限 / 作成者更新者の自動記録 / 行レベルセキュリティ / 自分宛タスク など)。初期ユーザー: admin/admin、alice/test、bob/test、carol/test、dave/test。\n※Visual Studio で新規ソリューションを作成するときは「Codeer.LowCode.Blazor.Cookie」で作成してください。その他のものとは整合しません。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateInventoryManagement,
                Name = "在庫管理テンプレート",
                Description = "倉庫の入庫・出庫・棚卸し・発注など、在庫管理業務を一通り含む業務テンプレート。複数倉庫や商品マスタの扱いの参考に。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateSFA,
                Name = "営業支援 (SFA) テンプレート",
                Description = "顧客 / 商談 / 活動履歴 / 案件パイプラインなど、営業支援 (SFA) 業務を一通り含む業務テンプレート。営業案件の進捗管理の参考に。",
            });
            DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
            {
                Create = CreateProjectManagement,
                Name = "プロジェクト管理テンプレート",
                Description = "プロジェクト / タスク / 工数 / 進捗管理など、プロジェクト管理業務を一通り含む業務テンプレート。タスク階層やガントチャート的な可視化の参考に。",
            });

            base.OnStartup(e);

            MainWindow.Title = "LowCodeApp.WinForms";
            DesignerEnvironment.AddMainMenu(ImportModulesFromExcel, "Tools", "Import Module from Excel");
            DesignerEnvironment.AddMainMenu(ImportModulesFromDdTables, "Tools", "Import Modules from Database");
            DesignerEnvironment.AddMainMenu(ExportPageObject, "Tools", "Export PageObject");

            DesignerEnvironment.AddSolutionExplorerMenu(CreateDDL, SolutionExplorerMenuTarget.Module, "Create DDL");
            DesignerEnvironment.AddSolutionExplorerMenu(CreateFieldDataClass, SolutionExplorerMenuTarget.Module, "Create FieldData Class");
            DesignerEnvironment.AddSolutionExplorerMenu(CreateEfClass, SolutionExplorerMenuTarget.Module, "Create EF Class");

            if (!string.IsNullOrEmpty(AISettings.Instance.OpenAIKey))
            {
                DesignerEnvironment.CreateQueryChat = editor => new QueryChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.CreateExecuteSqlChat = editor => new ExecuteSqlChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.CreateDetailLayoutChat = editor => new DetailLayoutChat(AISettings.Instance, editor);
                DesignerEnvironment.CreateSearchLayoutChat = editor => new SearchLayoutChat(AISettings.Instance, editor);
                DesignerEnvironment.CreateListLayoutChat = editor => new ListLayoutChat(AISettings.Instance, editor);
                DesignerEnvironment.CreateOverallSettingsChat = editor => new OverallSettingsChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.CreateCssEditorChat = editor => new CssChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.CreateScriptEditorChat = editor => new ScriptChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.CreatePageFrameChat = editor => new PageFrameChat(DesignerEnvironment, AISettings.Instance, editor);
                DesignerEnvironment.AddMainMenu(CreateModulesByAI, "Tools", "Create Modules by AI");
                DesignerEnvironment.AddSolutionExplorerMenu(e => CreateDBInformation(e, true), SolutionExplorerMenuTarget.Module, "Create DB Name (AI)", "All");
                DesignerEnvironment.AddSolutionExplorerMenu(e => CreateDBInformation(e, false), SolutionExplorerMenuTarget.Module, "Create DB Name (AI)", "Empty Only");
            }

            DesignerEnvironment.AddSolutionExplorerMenu(ExportPrintExcelCheatSheet, SolutionExplorerMenuTarget.Module, "Export Excel Print CheatSheet");

            DesignerEnvironment.AddDbColumnTransformHandler(DbColumnTransformHandler);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"An unhandled exception occurred:  {e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        FieldDesignBase? DbColumnTransformHandler(DataSource dataSource, DbTableDefinition table, string columnName)
        {
            var col = table.Columns.FirstOrDefault(e => e.Name == columnName);
            if (col == null) return null;
            if (dataSource.DataSourceType == DataSourceType.PostgreSQL && col.Name == "xmin")
                return new OptimisticLockingFieldDesign { Name = SystemFieldNames.OptimisticLocking, DbColumn = columnName };
            return null;
        }

        private void ImportModulesFromDdTables()
        {
            if (string.IsNullOrEmpty(DesignerEnvironment.CurrentFileDirectory)) return;

            //ユーザの選択をゲットする
            var datasourceToTables = DesignerEnvironment.GetDesignerSettings().DataSources.ToDictionary(e => e.Name, e => DesignerEnvironment.GetDbInfo(e.Name));
            var userSelected = DbTableSelectWindow.ShowDialog(datasourceToTables);
            if (userSelected == null) return;

            //モジュールに変換
            var err = DbTableParser.Import(DesignerEnvironment, userSelected.Value.selectedDataSource, userSelected.Value.selectedTables);
            if (!string.IsNullOrEmpty(err)) DesignerEnvironment.ShowToast(err, false);
        }

        private void CreateDBInformation(SolutionExplorerMenuClickEventArgs e, bool isAll)
        {
            var dlg = new WaitingWindow();
            dlg.Loaded += async (_, _) =>
            {
                var modName = e.Item.Split(".").First();
                var mod = DesignerEnvironment.GetDesignData().Modules.Find(modName);
                if (mod == null)
                {
                    DesignerEnvironment.ShowToast("Module not found", false);
                    return;
                }
                var settingsFile = File.ReadAllText(Path.Combine(DesignerEnvironment.CurrentFileDirectory, "designer.settings.json"));
                var clone = mod.JsonClone();
                await DbNameCreator.CreateDbNames(DesignerEnvironment.GetDesignerSettings(), clone, isAll);

                var path = Directory.GetFiles(Path.Combine(DesignerEnvironment.CurrentFileDirectory, "Modules"), e.Item, SearchOption.AllDirectories).FirstOrDefault() ??
                    Path.Combine(DesignerEnvironment.CurrentFileDirectory, "Modules", e.Item);

                File.WriteAllText(path, JsonConverterEx.SerializeObject(clone)); dlg.Close();
            };
            dlg.ShowDialog();
        }

        private void CreateDDL(SolutionExplorerMenuClickEventArgs e)
        {
            var modName = e.Item.Split(".").First();
            var mod = DesignerEnvironment.GetDesignData().Modules.Find(modName);
            if (mod == null)
            {
                DesignerEnvironment.ShowToast("Module not found", false);
                return;
            }
            var config = DesignerEnvironment.GetDesignerSettings();
            var dataSource = config.DataSources.FirstOrDefault(e => e.Name == mod.DataSourceName);
            if (dataSource == null)
            {
                DesignerEnvironment.ShowToast("Invalid Data Source", false);
                return;
            }

            var ddl = mod.CreateDDL(dataSource.DataSourceType);

            new DDLWindow
            {
                DesignerEnvironment = DesignerEnvironment,
                DataSource = dataSource,
                DisplayText = string.Join(Environment.NewLine, ddl),
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "DDL",
            }.Show();
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

        private void CreateEfClass(SolutionExplorerMenuClickEventArgs e)
        {
            var modName = e.Item.Split(".").First();
            var mod = DesignerEnvironment.GetDesignData().Modules.Find(modName);
            if (mod == null)
            {
                DesignerEnvironment.ShowToast("Module not found", false);
                return;
            }
            var config = DesignerEnvironment.GetDesignerSettings();
            var dataSource = config.DataSources.FirstOrDefault(e => e.Name == mod.DataSourceName);
            if (dataSource == null)
            {
                DesignerEnvironment.ShowToast("Invalid Data Source", false);
                return;
            }

            var classTxt = ClassGenerator.ModuleDesignToEfClass(mod, dataSource.DataSourceType);

            new TextDisplayWindow
            {
                DisplayText = classTxt,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "Module to EF Class",
            }.Show();
        }

        private void ExportPrintExcelCheatSheet(SolutionExplorerMenuClickEventArgs e)
        {
            if (string.IsNullOrEmpty(DesignerEnvironment.CurrentFileDirectory))
            {
                return;
            }

            var modName = e.Item.Split(".").First();
            var designData = DesignerEnvironment.GetDesignData();
            var module = designData.Modules.Find(modName);
            if (module == null)
            {
                DesignerEnvironment.ShowToast("Module not found", false);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"{module.Name}_PrintExcelCheatSheet.xlsx"
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var moduleToCheatSheet = new ModuleToExcelCheatSheet();
                var stream = moduleToCheatSheet.CreatePrintExcelCheatSheet(designData, module);
                File.WriteAllBytes(dialog.FileName, stream.ToArray());
                Process.Start(new ProcessStartInfo
                {
                    FileName = dialog.FileName,
                    UseShellExecute = true
                });
                DesignerEnvironment.ShowToast("Print Excel CheatSheet exported.", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private class AITextAnalyzerCoreDummy : IAITextAnalyzerCore
        {
            public Task<ModuleData?> FileToModuleDataAsync(string moduleName, string fieldName, string fileName, StreamContent content)
                => throw new NotImplementedException();

            public Task<ModuleData?> TextToModuleDataAsync(string moduleName, string fieldName, string text)
                => throw new NotImplementedException();
        }

        private void ImportModulesFromExcel()
        {
            if (string.IsNullOrEmpty(DesignerEnvironment.CurrentFileDirectory))
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var ddl = new ExcelImporter
                {
                    ProjectPath = DesignerEnvironment.CurrentFileDirectory
                }.Import(dialog.FileName);
                if (string.IsNullOrEmpty(ddl)) return;

                new TextDisplayWindow
                {
                    DisplayText = ddl,
                    Owner = MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Title = "DDL",
                }.Show();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CreateModulesByAI()
        {
            if (string.IsNullOrEmpty(DesignerEnvironment.CurrentFileDirectory)) return;
            var window = new AIChatWindow(new ModuleCreator(DesignerEnvironment, AISettings.Instance))
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.Show();
        }

        private void ExportPageObject()
        {
            if (string.IsNullOrEmpty(DesignerEnvironment.CurrentFileDirectory))
            {
                return;
            }

            var nameInputDialog = new NameInputDialog
            {
                Owner = MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (nameInputDialog.ShowDialog() != true)
            {
                return;
            }

            var ns = nameInputDialog.NameText;

            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() != true)
            {
                return;
            }

            var target = folderDialog.FolderName;
            var designData = DesignerEnvironment.GetDesignData();
            new SeleniumPageObjectBuilder
            {
                TargetPath = target,
                Namespace = ns,
            }.Build(designData);

            DesignerEnvironment.ShowToast("PageObject exported", true);
        }

        static void CreateEmpty(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.EmptyTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample.db", LowCodeApp.WinForms.Designer.Properties.Resources.sqlite_sample);
        }

        static void CreateEmptyAuth(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.EmptyAuthTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample_auth.db", LowCodeApp.WinForms.Designer.Properties.Resources.sqlite_sample_auth);
        }

        static void CreateGettingStandard(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.GettingStartedTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample.db", LowCodeApp.WinForms.Designer.Properties.Resources.sqlite_sample);
        }

        static void CreateInventoryManagement(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.InventoryManagementTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\inventory_v1.db", LowCodeApp.WinForms.Designer.Properties.Resources.inventory_v1);
        }

        static void CreateSFA(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.SFATemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sfa_v1.db", LowCodeApp.WinForms.Designer.Properties.Resources.sfa_v1);
        }

        static void CreateProjectManagement(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.ProjectManagementTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\project_management_v1.db", LowCodeApp.WinForms.Designer.Properties.Resources.project_management_v1);
        }

        static void CreatePatternShowcase(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.PatternShowcaseTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_patterns_v2.db", LowCodeApp.WinForms.Designer.Properties.Resources.sqlite_patterns_v2);
        }

        static void CreatePatternShowcaseAuth(string path)
        {
            using Stream stream = new MemoryStream(LowCodeApp.WinForms.Designer.Properties.Resources.PatternShowcaseAuthTemplate);
            ZipFile.ExtractToDirectory(stream, path);
            EnsureSampleDbExtracted("C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_patterns_auth_v2.db", LowCodeApp.WinForms.Designer.Properties.Resources.sqlite_patterns_auth_v2);
        }

        /// <summary>
        /// サンプル DB を展開する。ファイル無し、または 0 byte (Server が空ファイルを先に作っていた場合)
        /// なら上書き。ファイルが Server にロックされていて書けないときは案内メッセージを表示。
        /// </summary>
        static void EnsureSampleDbExtracted(string dbPath, byte[] data)
        {
            if (File.Exists(dbPath) && new FileInfo(dbPath).Length > 0) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                File.WriteAllBytes(dbPath, data);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(
                    $"サンプル DB の展開に失敗しました。Server プロジェクトが起動中の場合は停止してから新規作成をやり直してください。\n\n対象: {dbPath}\nエラー: {ex.Message}",
                    "DB 展開エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
