using System.Text;
using ClosedXML.Excel;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using genddl;
using static genddl.DbMapping;
using static genddl.FieldMapping;
using static genddl.Strings;

bool IsRowUsed(IXLRow row) => (!row.Cell(2).Value.IsBlank);

string CellValue(IXLCell cell) => cell.Value.IsBlank ? "" : cell.GetText();

var databaseType = "sqlserver";
var inputFile = "";
var outputDirectory = ".";
var ddlFileName = "ddl.txt";
var dataSourceName = "";

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
for (var i = 0; i < workbook.Worksheets.Count; i++)
{
    var worksheet = workbook.Worksheets.Worksheet(i + 1);
    var tableName = worksheet.Name;
    var moduleName = PascalCase(tableName);

    var definitions = worksheet.RowsUsed(IsRowUsed)
        .Select(r => new Row(CellValue(r.Cell(1)), CellValue(r.Cell(2)),
            r.Cells(3, 128).TakeWhile(c => !c.Value.IsBlank).Select(CellValue).ToArray())).ToList();

    // DDL
    var databaseColumnDefinitions = definitions.Where(r => !string.IsNullOrEmpty(r.name))
        .Select(r => $"  {r.name} {MapToColumnType(databaseType, r.type)}");
    ddl.AppendLine($"CREATE TABLE {tableName} (");
    ddl.AppendLine(string.Join(",\n", databaseColumnDefinitions));
    ddl.AppendLine(")");
    ddl.AppendLine("");

    // Module
    var module = new ModuleDesign
    {
        Name = moduleName,
        DataSourceName = dataSourceName,
        DbTable = tableName,
    };
    foreach (var def in definitions)
    {
        module.Fields.Add(MapToFieldDesign(def.name, def.type, def.args));
        if (def.type == "RadioGroup")
        {
            module.Fields.AddRange(MapRadioButtons(def.name, def.args));
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
        switch (def.type)
        {
            case "RadioGroup":
                defaultLayout.AddRadioGroup(module, def.name, def.args.Length);
                break;
            case "List":
                defaultLayout.AddList(def.name, def.args.GetOrDefault(0));
                break;
            default:
                defaultLayout.AddField(module, def.name);
                break;
        }
    }

    defaultLayout.AddSubmitLayout(module);

    var listLayout = module.ListLayouts[""];
    listLayout.Elements =
    [
        definitions.Where(def => (def.type != "Id") && (def.type != "List")).Select(def => new ListElement
        {
            FieldName = PascalCase(def.name),
        }).ToList()
    ];

    File.WriteAllText(Path.Combine(outputDirectory, $"{moduleName}.mod.json"), JsonConverterEx.SerializeObject(module));
}

File.WriteAllText(Path.Combine(outputDirectory, $"{ddlFileName}"), ddl.ToString());

record Row(string name, string type, string[] args);
