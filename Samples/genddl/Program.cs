using System.Text;
using ClosedXML.Excel;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using genddl;
using static genddl.DbMapping;
using static genddl.FieldMapping;
using static genddl.Strings;

var databaseType = "sqlserver";
var inputFile = "";
var outputDirectory = ".";
var ddlFileName = "ddl.txt";
var dataSourceName = "";
var projectPath = "";


bool IsRowUsed(IXLRow row) => (!row.Cell(2).Value.IsBlank);

string CellValue(IXLCell cell) => cell.Value.IsBlank ? "" : cell.GetText();

string GetDdlOutputPath()
{
    if (string.IsNullOrEmpty(projectPath)) return outputDirectory;
    return projectPath;
}

string GetModuleOutputPath()
{
    if (string.IsNullOrEmpty(projectPath)) return outputDirectory;
    return Path.Combine(projectPath, "Modules");
}

string GetPageFrameOutputPath()
{
    if (string.IsNullOrEmpty(projectPath)) return outputDirectory;
    return Path.Combine(projectPath, "PageFrames");
}

for (var i = 0; i < args.Length; ++i)
{
    switch (args[i])
    {
        case "--db":
            databaseType = args.GetOrDefault(++i);
            continue;
        case "--output":
            outputDirectory = args.GetOrDefault(++i);
            continue;
        case "--ddl":
            ddlFileName = args.GetOrDefault(++i);
            continue;
        case "--datasource":
            dataSourceName = args.GetOrDefault(++i);
            continue;
        case "--clprj":
            projectPath = args.GetOrDefault(++i);
            if (projectPath.EndsWith(".clprj"))
            {
                projectPath = Path.GetDirectoryName(projectPath);
            }
            continue;
        case "--help":
        case "-help":
        case "/?":
            Commands.PrintHelp();
            return;
    }

    inputFile = args[i];
    break;
}

if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
{
    Console.WriteLine("Input file not found.");
    return;
}

if (string.IsNullOrEmpty(dataSourceName))
{
    dataSourceName = Path.GetFileNameWithoutExtension(inputFile);
}

if (!string.IsNullOrEmpty(outputDirectory))
{
    try
    {
        Directory.CreateDirectory(outputDirectory);
    }
    catch { }
}

var workbook = new XLWorkbook(inputFile);
var ddl = new StringBuilder();
var modules = new List<string>();
for (var i = 0; i < workbook.Worksheets.Count; i++)
{
    var worksheet = workbook.Worksheets.Worksheet(i + 1);
    var tableName = worksheet.Name;
    var moduleName = PascalCase(tableName);

    var definitions = worksheet.RowsUsed(IsRowUsed)
        .Select(r => new Def(CellValue(r.Cell(1)), CellValue(r.Cell(2)),
            r.Cells(3, 128).TakeWhile(c => !c.Value.IsBlank).Select(CellValue).ToArray())).ToList();

    // DDL
    var databaseColumnDefinitions = definitions.Where(r => !string.IsNullOrEmpty(r.Name))
        .Select(r => $"  {r.Name} {MapToColumnType(databaseType, r.Type)}");
    ddl.AppendLine($"CREATE TABLE {tableName} (");
    ddl.AppendLine(string.Join(",\n", databaseColumnDefinitions));
    ddl.AppendLine(");");
    ddl.AppendLine("");

    // Module
    modules.Add(moduleName);
    var module = new ModuleDesign
    {
        Name = moduleName,
        DataSourceName = dataSourceName,
        DbTable = tableName,
    };
    foreach (var def in definitions)
    {
        module.Fields.Add(MapToFieldDesign(def.Name, def.Type, def.Args));
        if (def.Type == "RadioGroup")
        {
            module.Fields.AddRange(MapRadioButtons(def.Name, def.Args));
        }
    }

    var defaultLayout = module.DetailLayouts[""];
    // Clear layout
    defaultLayout.Layout = new GridLayoutDesign()
    {
        Rows = []
    };

    defaultLayout.AddHeaderLayout(module, tableName);
    foreach (var def in definitions)
    {
        switch (def.Type)
        {
            case "Id":
                break;
            case "RadioGroup":
                defaultLayout.AddRadioGroup(module, def.Name, def.Args.Length);
                break;
            case "List":
                defaultLayout.AddList(def.Name, def.Args.GetOrDefault(0));
                break;
            default:
                defaultLayout.AddField(module, def.Name);
                break;
        }
    }

    defaultLayout.AddSubmitLayout(module);

    var listLayout = module.ListLayouts[""];
    listLayout.Elements =
    [
        definitions.Where(def => (def.Type != "Id") && (def.Type != "List")).Select(def => new ListElement
        {
            FieldName = PascalCase(def.Name),
        }).ToList()
    ];

    File.WriteAllText(Path.Combine(GetModuleOutputPath(), $"{moduleName}.mod.json"), JsonConverterEx.SerializeObject(module));
}

File.WriteAllText(Path.Combine(GetDdlOutputPath(), $"{ddlFileName}"), ddl.ToString());

var pageFrame = new PageFrameDesign();
try
{
    var pageFrameJson = File.ReadAllText(Path.Combine(GetPageFrameOutputPath(), "Main.frm.json"));
    pageFrame = JsonConverterEx.DeserializeObject<PageFrameDesign>(pageFrameJson) ?? new PageFrameDesign();
}
catch { }

foreach (var module in modules)
{
    pageFrame.Left.Links.Add(new PageLink
    {
        Module = module,
        Title = module,
    });
}

File.WriteAllText(Path.Combine(GetPageFrameOutputPath(), "Main.frm.json"),
    JsonConverterEx.SerializeObject(pageFrame));

internal record Def(string Name, string Type, string[] Args);
