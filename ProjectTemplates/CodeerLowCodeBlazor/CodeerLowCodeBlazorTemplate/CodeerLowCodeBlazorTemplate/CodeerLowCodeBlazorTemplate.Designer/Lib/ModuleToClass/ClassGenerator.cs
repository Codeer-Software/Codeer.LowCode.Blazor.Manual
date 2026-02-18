using System.Reflection;
using System.Text;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.SystemSettings;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib.ModuleToClass
{
    internal class ClassGenerator
    {
        internal static string ModuleDesignToDataFieldClass(ModuleDesign mod)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"public class {mod.Name}");
            stringBuilder.AppendLine("{");

            foreach (var field in mod.Fields)
            {
                //DbColumnがついているFieldのみ
                var designType = field.GetType();
                if (GetDbColumnPropertyInfo(designType) == null) continue;
                if (IsLinkField(field.Name)) continue;

                var designTypeName = field.TypeFullName;
                designTypeName = designTypeName.Substring(designTypeName.LastIndexOf(".") + 1);

                var dataTypeName = designTypeName.Replace("FieldDesign", "FieldData");
                stringBuilder.AppendLine($"\tpublic {dataTypeName}? {field.Name} {{get; set;}}");
            }

            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }
        internal static string ModuleDesignToEfClass(ModuleDesign mod, DataSourceType dataSourceType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"[Table(\"{mod.DbTable}\")]");
            stringBuilder.AppendLine($"public class {char.ToUpper(mod.Name[0])}{mod.Name.Substring(1)}");
            stringBuilder.AppendLine("{");

            foreach (var field in mod.Fields)
            {
                //DbColumnがついているFieldのみ
                var designType = field.GetType();
                var dbColumnProperty = GetDbColumnPropertyInfo(designType);
                if (dbColumnProperty == null) continue;
                if (IsLinkField(field.Name)) continue;

                var dbColumnValue = dbColumnProperty.GetValue(field)!.ToString();

                if (DotNetTypeMapping.TryGetValue(designType, out var dotNetType) == false)
                    dotNetType = typeof(string).Name;

                stringBuilder.AppendLine($"\t[Column(\"{dbColumnValue}\")]");
                stringBuilder.AppendLine($"\tpublic {dotNetType}? {field.Name} {{get; set;}}");
            }

            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private static bool IsLinkField(string fieldName)
        {
            var nameObj = new FieldName(fieldName);
            return nameObj.IsLink;
        }
        private static PropertyInfo? GetDbColumnPropertyInfo(Type designType)
        {
            //"DbColumn"というProperty名と限らないため、Atrributeで判断する
            var properties = designType.GetProperties();
            if (properties == null) return null;
            if (properties.Any() == false) return null;
            return properties.FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DbColumnAttribute)));
        }
        private static readonly Dictionary<Type, string> DotNetTypeMapping = new()
        {
            {typeof(IdFieldDesign), "long"},
            {typeof(TextFieldDesign), typeof(string).Name.ToLower()},
            {typeof(NumberFieldDesign), "int"},
            {typeof(DateFieldDesign), typeof(DateOnly).Name},
            {typeof(DateTimeFieldDesign), typeof(DateTime).Name},
            {typeof(TimeFieldDesign), typeof(TimeOnly).Name},
            {typeof(BooleanFieldDesign), "bool"},
            {typeof(LinkFieldDesign), typeof(string).Name.ToLower()},
            {typeof(SelectFieldDesign), typeof(string).Name.ToLower()},
            {typeof(RadioGroupFieldDesign), typeof(string).Name.ToLower()}
        };
    }
}
