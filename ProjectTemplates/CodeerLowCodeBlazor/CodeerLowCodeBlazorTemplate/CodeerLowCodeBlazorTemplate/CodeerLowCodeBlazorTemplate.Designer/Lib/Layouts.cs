using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Design;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib
{
    internal static class Layouts
    {
        internal static void CreateLayouts(this ModuleDesign module)
        {
            var defaultLayout = module.DetailLayouts[""];
            defaultLayout.Layout = new GridLayoutDesign();
            var listLayout = module.ListLayouts[""];
            listLayout.Elements.Clear();
            listLayout.Elements.Add(new());

            defaultLayout.AddHeaderLayout(module);
            foreach (var field in module.Fields.ToList())
            {
                if (field is IdFieldDesign) continue;
                else if (field is LabelFieldDesign) continue;
                else if (field is RadioButtonFieldDesign) continue;
                else if (field is RadioGroupFieldDesign radio)
                {
                    defaultLayout.AddRadioGroup(module, radio);
                }
                else if (field is ListFieldDesign list)
                {
                    defaultLayout.AddList(list);
                    continue;
                }
                else
                {
                    defaultLayout.AddField(module, field);
                }
                if (field is LinkFieldDesign link)
                {
                    if (string.IsNullOrEmpty(link.DisplayTextVariable)) continue;
                    if (link.DisplayTextVariable == "Id.Value") continue;
                }
                listLayout.Elements[0].Add(new() { FieldName = field.Name });
            }

            defaultLayout.AddSubmitLayout(module);
        }

        private static void AddHeaderLayout(this DetailLayoutDesign design, ModuleDesign module)
        {
            var field = new LabelFieldDesign()
            {
                Name = "Header",
                Text = module.Name,
                Style = LabelStyle.H1
            };
            module.Fields.Add(field);

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign("Header"),
                    HorizontalAlignment = HorizontalAlignment.Center
                }]
            });
        }

        private static void AddSubmitLayout(this DetailLayoutDesign design, ModuleDesign module)
        {
            var field = new SubmitButtonFieldDesign
            {
                Name = "Submit"
            };
            module.Fields.Add(field);

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign("Submit"),
                    HorizontalAlignment = HorizontalAlignment.Center
                }]
            });
        }

        private static void AddField(this DetailLayoutDesign design, ModuleDesign module, FieldDesignBase field)
        {
            var labelField = new LabelFieldDesign()
            {
                Name = field.Name + "Label",
                Text = string.Empty,
                RelativeField = field.Name
            };
            module.Fields.Add(labelField);

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign(field.Name + "Label"),
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Middle
                }, new GridColumn()
                {
                    Layout = new FieldLayoutDesign(field.Name),
                }]
            });
        }

        private static void AddRadioGroup(this DetailLayoutDesign design, ModuleDesign module, RadioGroupFieldDesign radio)
        {
            var field = new LabelFieldDesign()
            {
                Name = radio.Name + "Label",
                Text = radio.Name
            };
            module.Fields.Add(field);
            design.DataOnlyFields.Add(radio.Name);

            var radioButtonCount = module.Fields.OfType<RadioButtonFieldDesign>().Where(e => e.GroupField == radio.Name).Count();

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns =
                [
                    new GridColumn
                    {
                        Layout = new FieldLayoutDesign(radio.Name + "Label"),
                        Width = 150,
                        VerticalAlignment = VerticalAlignment.Middle
                    },
                    new GridColumn
                    {
                        Layout = new GridLayoutDesign
                        {
                            IsFlowLayout = true,
                            Rows =
                            [
                                new GridRow
                                {
                                    Columns = Enumerable.Range(0, radioButtonCount).Select(i => new GridColumn
                                    {
                                        Layout = new FieldLayoutDesign(radio.Name + "Item" + i)
                                    }).ToList()
                                }
                            ]
                        }
                    }
                ]
            });
        }

        private static void AddList(this DetailLayoutDesign design, ListFieldDesign list)
        {
            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign(list.Name),
                }]
            });
        }

    }
}
