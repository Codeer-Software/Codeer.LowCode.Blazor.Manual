using Codeer.LowCode.Blazor.Repository.Design;
using static genddl.Strings;

namespace genddl
{
    internal static class Layouts
    {
        public static void AddHeaderLayout(this DetailLayoutDesign design, ModuleDesign module, string name)
        {
            var field = new LabelFieldDesign()
            {
                Name = "Header",
                Text = PascalCase(name),
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

        public static void AddSubmitLayout(this DetailLayoutDesign design, ModuleDesign module)
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

        public static void AddField(this DetailLayoutDesign design, ModuleDesign module, string name)
        {
            var field = new LabelFieldDesign()
            {
                Name = PascalCase(name) + "Label",
                Text = PascalCase(name)
            };
            module.Fields.Add(field);

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign(PascalCase(name) + "Label"),
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Middle
                }, new GridColumn()
                {
                    Layout = new FieldLayoutDesign(PascalCase(name)),
                }]
            });
        }

        public static void AddRadioGroup(this DetailLayoutDesign design, ModuleDesign module, string name, int howMany)
        {
            var field = new LabelFieldDesign()
            {
                Name = PascalCase(name) + "Label",
                Text = PascalCase(name)
            };
            module.Fields.Add(field);
            design.DataOnlyFields.Add(PascalCase(name));

            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns =
                [
                    new GridColumn
                    {
                        Layout = new FieldLayoutDesign(PascalCase(name) + "Label"),
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
                                    Columns = Enumerable.Range(0, howMany).Select(i => new GridColumn
                                    {
                                        Layout = new FieldLayoutDesign(PascalCase(name) + "Item" + i)
                                    }).ToList()
                                }
                            ]
                        }
                    }
                ]
            });
        }

        public static void AddList(this DetailLayoutDesign design, string name, string module)
        {
            ((GridLayoutDesign)design.Layout).Rows.Add(new GridRow
            {
                Columns = [new GridColumn
                {
                    Layout = new FieldLayoutDesign(PascalCase(name) + "List" + module),
                }]
            });
        }
    }
}
