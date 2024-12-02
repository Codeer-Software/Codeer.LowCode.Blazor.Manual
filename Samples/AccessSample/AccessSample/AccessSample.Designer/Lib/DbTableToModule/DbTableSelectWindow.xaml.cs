using Codeer.LowCode.Blazor.DataIO.Db.Definition;
using MahApps.Metro.Controls;
using System.Windows;

namespace AccessSample.Designer.Lib.DbTableToModule
{
    public partial class DbTableSelectWindow : MetroWindow
    {
        private class ListItem
        {
            public bool IsChecked { get; set; }
            public string Name => DbTableDefinition.Name;
            public DbTableDefinition DbTableDefinition { get; set; } = new DbTableDefinition();
        }

        DbTableSelectWindow(Dictionary<string, List<DbTableDefinition>> sourceTableDic)
        {
            InitializeComponent();

            _comboDataSource.ItemsSource = sourceTableDic.Keys;
            _comboDataSource.SelectionChanged += (s, _) =>
                _listTables.ItemsSource = sourceTableDic.TryGetValue(_comboDataSource.SelectedItem.ToString() ?? string.Empty, out var table) ?
                table.Select(e => new ListItem { DbTableDefinition = e }).ToList() : new List<ListItem>();
            _comboDataSource.SelectedIndex = sourceTableDic.Any() ? 0 : -1;
        }

        public static (string selectedDataSource, List<DbTableDefinition> selectedTables)? ShowDialog(Dictionary<string, List<DbTableDefinition>> sourceTableDictionary)
        {
            var instance = new DbTableSelectWindow(sourceTableDictionary);
            instance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            instance.Owner = Application.Current.MainWindow;

            if (instance.ShowDialog() != true) return null;
            return (instance._comboDataSource.Text, instance._listTables.ItemsSource.Cast<ListItem>()
                .Where(e => e.IsChecked).Select(e => e.DbTableDefinition).ToList());
        }

        void OKClick(object sender, System.Windows.RoutedEventArgs e) => DialogResult = true;

        void CancelClick(object sender, System.Windows.RoutedEventArgs e) => DialogResult = false;
    }
}
