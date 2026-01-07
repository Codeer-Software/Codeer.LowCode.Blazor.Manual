using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Design;

namespace LowCodeSamples.Client.Shared.Fields
{
    static class DesignHelper
    {
        internal static string? Or(this string? that, string? then) => string.IsNullOrEmpty(that) ? then : that;

        internal static string? GetLayoutColor(this FieldBase field)
        {
            switch (field.ModuleLayoutType)
            {
                case ModuleLayoutType.List:
                    {
                        if (!field.Module.Design.ListLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Elements.SelectMany(e => e).FirstOrDefault(e => e.FieldName == field.Design.Name)?.Color;
                    }
                case ModuleLayoutType.Detail:
                    {
                        if (!field.Module.Design.DetailLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Layout.GetDescendantFields().FirstOrDefault(e => e.FieldName == field.Design.Name)?.Color;
                    }
                case ModuleLayoutType.Search:
                    {
                        if (!field.Module.Design.SearchLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Layout.GetDescendantFields().FirstOrDefault(e => e.FieldName == field.Design.Name)?.Color;
                    }
                default:
                    return null;
            }
        }

        internal static string? GetBackgroundColor(this FieldBase field)
        {
            switch (field.ModuleLayoutType)
            {
                case ModuleLayoutType.List:
                    {
                        if (!field.Module.Design.ListLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Elements.SelectMany(e => e).FirstOrDefault(e => e.FieldName == field.Design.Name)?.BackgroundColor;
                    }
                case ModuleLayoutType.Detail:
                    {
                        if (!field.Module.Design.DetailLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Layout.GetDescendantFields().FirstOrDefault(e => e.FieldName == field.Design.Name)?.BackgroundColor;
                    }
                case ModuleLayoutType.Search:
                    {
                        if (!field.Module.Design.SearchLayouts.TryGetValue(field.LayoutName, out var layout)) return null;
                        return layout.Layout.GetDescendantFields().FirstOrDefault(e => e.FieldName == field.Design.Name)?.BackgroundColor;
                    }
                default:
                    return null;
            }
        }

        internal static IEnumerable<FieldLayoutDesign> GetDescendantFields(this LayoutDesignBase layout) =>
            layout.GetDescendantLayouts().OfType<FieldLayoutDesign>();

        internal static IEnumerable<CanvasLayoutDesign> GetDescendantCanvas(this LayoutDesignBase layout) =>
            layout.GetDescendantLayouts().OfType<CanvasLayoutDesign>();


        internal static List<LayoutDesignBase> GetDescendantLayouts(this LayoutDesignBase? layout)
        {
            var list = new List<LayoutDesignBase>();
            if (layout != null) list.Add(layout);

            if (layout is GridLayoutDesign grid)
            {
                foreach (var row in grid.Rows)
                {
                    foreach (var col in row.Columns)
                    {
                        list.AddRange(col.Layout.GetDescendantLayouts());
                    }
                }
            }

            if (layout is TabLayoutDesign tabLayout)
            {
                foreach (var tab in tabLayout.Layouts)
                {
                    list.AddRange(tab.GetDescendantLayouts());
                }
            }

            if (layout is CanvasLayoutDesign canvas)
            {
                foreach (var col in canvas.Elements)
                {
                    list.AddRange(col.Layout.GetDescendantLayouts());
                }
            }

            return list;
        }

    }
}
