using ClosedXML.Excel;
using Codeer.LowCode.Blazor.Designer.Config;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.SystemSettings;
using System.Diagnostics;
using System.IO;

namespace AccessSample.Designer.Lib.ExcelToModule
{
    internal class ExcelImporter
    {
        internal string ProjectPath { get; set; } = "";

        private bool IsRowUsed(IXLRow row) => (!row.Cell(2).Value.IsBlank);
        private string CellValue(IXLCell cell) => cell.Value.IsBlank ? "" : cell.GetText();

        internal string Import(string inputFile)
        {
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                Debug.WriteLine("Input file not found.");
                return string.Empty;
            }

            //DataSource
            var dataSourceName = Path.GetFileNameWithoutExtension(inputFile);
            DataSourceType dataSourceType = DataSourceType.SQLite;
            try
            {
                var settingsFile = File.ReadAllText(Path.Combine(ProjectPath, "designer.settings.json"));
                var config = JsonConverterEx.DeserializeObject<DesignerSettings>(settingsFile)!;
                var dataSource = config.DataSources.FirstOrDefault(ds => ds.Name == dataSourceName);
                dataSourceType = dataSource?.DataSourceType ?? DataSourceType.SQLite;
            }
            catch { }

            XLWorkbook workbook;
            using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                workbook = new XLWorkbook(fileStream);
            }

            var ddl = new List<string>();
            var modules = new List<ModuleDesign>();
            for (var i = 0; i < workbook.Worksheets.Count; i++)
            {
                //get settings form excel
                var worksheet = workbook.Worksheets.Worksheet(i + 1);
                var sheetNameSep = worksheet.Name.IndexOf("|");
                var moduleName = worksheet.Name;
                var tableName = worksheet.Name;
                if (sheetNameSep != -1)
                {
                    moduleName = worksheet.Name.Substring(0, sheetNameSep);
                    tableName = worksheet.Name.Substring(sheetNameSep + 1);
                }
                var fieldDesignInfo = worksheet.RowsUsed(IsRowUsed)
                    .Select(r => new FieldMapping.DesignInfo(CellValue(r.Cell(1)), CellValue(r.Cell(2)),
                        r.Cells(3, 128).TakeWhile(c => !c.Value.IsBlank).Select(CellValue).ToArray())).ToList();

                // Module
                var module = new ModuleDesign
                {
                    Name = moduleName,
                    DataSourceName = dataSourceName,
                    DbTable = tableName,
                };
                modules.Add(module);
                foreach (var info in fieldDesignInfo)
                {
                    module.Fields.AddRange(FieldMapping.MapToFieldDesign(info));
                }
                module.CreateLayouts();

                //Write to mod.json file.
                File.WriteAllText(Path.Combine(GetModuleOutputPath(), $"{moduleName}.mod.json"),
                    JsonConverterEx.SerializeObject(module));

                // DDL
                if (ddl.Any()) ddl.Add(string.Empty);
                ddl.AddRange(module.CreateDDL(dataSourceType));
            }

            //Add to Main PageFrame.
            WriteToMain(Path.Combine(GetPageFrameOutputPath(), "Main.frm.json"), modules);

            return string.Join(Environment.NewLine, ddl);
        }

        internal void Import(List<ModuleDesign> mods)
        {
            foreach (var mod in mods)
            {
                mod.CreateLayouts();
                File.WriteAllText(Path.Combine(GetModuleOutputPath(), $"{mod.Name}.mod.json"),
                    JsonConverterEx.SerializeObject(mod));
            }
            WriteToMain(Path.Combine(GetPageFrameOutputPath(), "Main.frm.json"), mods);
        }

        private string GetDdlOutputPath() => ProjectPath;

        private string GetModuleOutputPath() => Path.Combine(ProjectPath, "Modules");

        private string GetPageFrameOutputPath() => Path.Combine(ProjectPath, "PageFrames");

        static void WriteToMain(string path, List<ModuleDesign> modules) => WriteToMain(path, modules.Select(e => e.Name).ToList());

        static void WriteToMain(string path, List<string> modules)
        {
            var pageFrame = new PageFrameDesign();
            try
            {
                var pageFrameJson = File.ReadAllText(path);
                pageFrame = JsonConverterEx.DeserializeObject<PageFrameDesign>(pageFrameJson) ?? new PageFrameDesign();
            }
            catch { }

            foreach (var module in modules)
            {
                if (pageFrame.Left.Links.Any(e => e.Module == module)) continue;
                pageFrame.Left.Links.Add(new PageLink
                {
                    Module = module,
                    Title = module,
                });
            }

            File.WriteAllText(path, JsonConverterEx.SerializeObject(pageFrame));
        }
    }
}
