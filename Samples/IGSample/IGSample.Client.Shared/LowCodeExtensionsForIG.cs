using Codeer.LowCode.Blazor.Repository.Design;
using IgniteUI.Blazor.Controls;
using Codeer.LowCode.Blazor.OperatingModel;
using System.Collections.Generic;

namespace IGSample.Client.Shared
{
    class IgColumn
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public GridColumnDataType Type { get; set; }
    }

    static class LowCodeExtensionsForIG
    {
        internal static List<IgColumn> GetIgColumn(this ModuleDesign x, string layoutName)
        {
            var result = new List<IgColumn>();
            foreach (var e in x.ListLayouts[layoutName].Elements.SelectMany(e => e))
            {
                var column = new IgColumn();
                var fieldDesign = x.Fields.FirstOrDefault(f => f.Name == e.FieldName);
                if (fieldDesign == null) continue;
                column.Name = fieldDesign.Name;
                column.DisplayName = !string.IsNullOrEmpty(e.Label) ? e.Label :
                    (fieldDesign is IDisplayName displayName) ? displayName.DisplayName : fieldDesign.Name;
                if (fieldDesign is NumberFieldDesign) column.Type = GridColumnDataType.Number;
                else if (fieldDesign is DateFieldDesign) column.Type = GridColumnDataType.Date;
                else if (fieldDesign is DateTimeFieldDesign) column.Type = GridColumnDataType.DateTime;
                else if (fieldDesign is TimeFieldDesign) column.Type = GridColumnDataType.Time;
                else if (fieldDesign is BooleanFieldDesign) column.Type = GridColumnDataType.Boolean;
                else column.Type = GridColumnDataType.String;
                result.Add(column);
            }

            return result;
        }


        internal static List<Dictionary<string, object?>> GetListData(this ListField field,
            Codeer.LowCode.Blazor.RequestInterfaces.Services services, string layoutName)
        {
            var formDesignName = field.Design.SearchCondition.ModuleName;
            if (string.IsNullOrEmpty(formDesignName)) return new();

            var module = services.AppInfoService.GetDesignData().Modules.FirstOrDefault(e => e.Name == formDesignName);
            if (module == null) return new();

            var ret = new List<Dictionary<string, object?>>();
            foreach (var row in field.Rows)
            {
                var dic = new Dictionary<string, object?>();
                foreach (var e in module.ListLayouts[layoutName].Elements.SelectMany(e => e))
                {
                    var f = row.GetField(e.FieldName);
                    if (f is TextField text) dic[e.FieldName] = text.Value;
                    else if (f is NumberField number) dic[e.FieldName] = number.Value;
                    else if (f is DateField date) dic[e.FieldName] = date.Value;
                    else if (f is DateTimeField dateTime) dic[e.FieldName] = dateTime.Value;
                    else if (f is TimeField time) dic[e.FieldName] = time.Value;
                    else if (f is BooleanField boolean) dic[e.FieldName] = boolean.Value;
                    else if (f is ListNumberField) dic[e.FieldName] = ret.Count + 1;
                    else if (f is IdField id) dic[e.FieldName] = id.Value;
                    else if (f is LinkField link) dic[e.FieldName] = link.Value;
                }

                ret.Add(dic);
            }

            return ret;
        }
    }
}
