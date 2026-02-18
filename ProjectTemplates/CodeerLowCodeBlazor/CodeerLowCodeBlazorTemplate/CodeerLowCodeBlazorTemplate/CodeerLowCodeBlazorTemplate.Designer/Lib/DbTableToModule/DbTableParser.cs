using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using Codeer.LowCode.Blazor.DataIO.Db.Definition;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib.DbTableToModule
{
    public static class DbTableParser
    {
        public static string Import(DesignerEnvironment designerEnvironment, string dataSourceName, List<DbTableDefinition> tables)
        {
            var modules = new List<string>();
            var err = new List<string>();
            foreach (var table in tables)
            {
                var module = new ModuleDesign()
                {
                    Name = DbNameToDesignName(table.Name),
                    DataSourceName = dataSourceName,
                    DbTable = table.Name
                };
                foreach (var col in table.Columns)
                {
                    var field = CreateFieldDesign(col);
                    field.Name = DbNameToDesignName(col.Name);
                    field.GetType().GetProperties().Where(e => e.GetCustomAttribute<DbColumnAttribute>() != null).FirstOrDefault()?.SetValue(field, col.Name);
                    module.Fields.Add(field);
                }
                module.CreateLayouts();

                try
                {
                    File.WriteAllTextAsync(Path.Combine(designerEnvironment.CurrentFileDirectory, "Modules", $"{module.Name}.mod.json"), JsonConverterEx.SerializeObject(module));
                    modules.Add(module.Name);
                }
                catch (Exception exp)
                {
                    err.Add(exp.Message);
                }
            }

            //ページフレームに追加
            var designData = designerEnvironment.GetDesignData();
            var pageFrame = designData.PageFrames.Find("Main");
            if (pageFrame == null) pageFrame = designData.PageFrames.ToList().FirstOrDefault();
            if (pageFrame == null) return string.Empty;
            foreach (var module in modules)
            {
                if (pageFrame.Left.Links.Any(e => e.Module == module)) continue;
                pageFrame.Left.Links.Add(new PageLink
                {
                    Module = module,
                    Title = module,
                });
            }
            var path = Path.Combine(designerEnvironment.CurrentFileDirectory, "PageFrames", $"{pageFrame.Name}.frm.json");
            try
            {
                File.WriteAllText(path, JsonConverterEx.SerializeObject(pageFrame));
            }
            catch (Exception exp)
            {
                err.Add(exp.Message);
            }
            return string.Join(Environment.NewLine, err);
        }

        static string DbNameToDesignName(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;

            var words = source.Split(['_', '.']);
            if (words.Length == 1)
            {
                if (string.IsNullOrEmpty(words[0])) return string.Empty;
                var first = CultureInfo.InvariantCulture.TextInfo.ToUpper(words[0].Substring(0, 1));
                if (words[0].Length == 1) return first;
                return first + words[0].Substring(1);
            }

            var capitalizedWords = words.Select(word =>
                CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower()));
            return string.Concat(capitalizedWords);
        }

        static FieldDesignBase CreateFieldDesign(DbColumnDefinition col)
        {
            if (col.Name.ToLower() == SystemFieldNames.Id.ToLower()) return new IdFieldDesign();

            Type? type;
            if (new[]
                {
                        typeof(byte).FullName!,
                        typeof(char).FullName!,
                        typeof(short).FullName!,
                        typeof(ushort).FullName!,
                        typeof(int).FullName!,
                        typeof(uint).FullName!,
                        typeof(long).FullName!,
                        typeof(ulong).FullName!,
                        typeof(Single).FullName!,
                        typeof(double).FullName!,
                        typeof(decimal).FullName!
                    }.Contains(col.NetTypeFullName))
            {
                type = typeof(NumberFieldDesign);
            }
            else if (typeof(bool).FullName! == col.NetTypeFullName)
            {
                type = typeof(BooleanFieldDesign);
            }
            else if (typeof(Guid).FullName! == col.NetTypeFullName)
            {
                type = typeof(TextFieldDesign);
            }
            else if (typeof(DateTime).FullName! == col.NetTypeFullName)
            {
                type = typeof(DateTimeFieldDesign);
            }
            else if (typeof(DateOnly).FullName! == col.NetTypeFullName)
            {
                type = typeof(DateFieldDesign);
            }
            else if (typeof(TimeOnly).FullName! == col.NetTypeFullName ||
                     (typeof(TimeSpan).FullName! == col.NetTypeFullName) ||
                     (typeof(DateTimeOffset).FullName! == col.NetTypeFullName))
            {
                type = typeof(TimeFieldDesign);
            }
            else type = typeof(TextFieldDesign);

            return (FieldDesignBase)Activator.CreateInstance(type)!;
        }
    }
}
