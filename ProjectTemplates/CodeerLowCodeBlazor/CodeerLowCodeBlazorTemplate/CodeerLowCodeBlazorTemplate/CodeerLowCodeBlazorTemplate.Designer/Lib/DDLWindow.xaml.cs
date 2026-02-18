using System.Data;
using System.IO;
using System.Windows.Input;
using System.Xml;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Designer.Views.Windows;
using Codeer.LowCode.Blazor.SystemSettings;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib
{
    public partial class DDLWindow : MetroWindow
    {
        DataSource _dataSource = new();
        public DataSource DataSource
        {
            get => _dataSource;
            set
            {
                _dataSource = value;
                InitXshd();
            }
        }
        public DesignerEnvironment? DesignerEnvironment { get; set; }

        public string DisplayText
        {
            get => _sqlEditor.Text;
            set => _sqlEditor.Text = value;
        }

        public static readonly RoutedUICommand RunCommand =
            new RoutedUICommand("Run", "RunCommand", typeof(DDLWindow));

        void RunCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        void RunCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => ExecuteRun();

        public DDLWindow()
            => InitializeComponent();

        void InitXshd()
        {
            var xshd = SqlEditHelper.SqlCommonXshd;
            switch (DataSource.DataSourceType)
            {
                case DataSourceType.SQLServer:
                    xshd = SqlEditHelper.SqlServerXshd;
                    break;
            }
            if (!string.IsNullOrEmpty(xshd))
            {
                using var reader = XmlReader.Create(new StringReader(xshd));
                var def = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                if (def != null)
                {
                    _sqlEditor.SyntaxHighlighting = def;
                }
            }
        }

        async void ExecuteRun()
        {
            try
            {
                await using var dbAccess = DesignerEnvironment!.ServiceProvider.GetRequiredService<IDbAccessorFactory>().Create([DataSource]);

                var conn = dbAccess.GetConnection(DataSource.Name);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = _sqlEditor.Text;
                cmd.CommandType = CommandType.Text;

                var result = await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
                return;
            }
            DesignerEnvironment.RefreshDatabase();
            DesignerEnvironment.ShowToast("Completed", true);
        }
    }
}
