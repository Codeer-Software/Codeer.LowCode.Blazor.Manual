using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace AccessSample.Designer.Lib.ExcelToModule
{
    internal static class FieldMapping
    {
        internal record DesignInfo(string Type, string FieldName, string[] Args);

        internal static List<FieldDesignBase> MapToFieldDesign(DesignInfo info)
        {
            return info.Type switch
            {
                "Id" => [new IdFieldDesign { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "Text" => [new TextFieldDesign { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "Number" => [new NumberFieldDesign { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "Date" => [new DateFieldDesign() { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "DateTime" => [new DateTimeFieldDesign() { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "Time" => [new TimeFieldDesign() { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "Boolean" => [new BooleanFieldDesign() { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0), }],
                "File" => [CreateFileFieldDesign(info)],
                "Link" => [CreateLinkFieldDesign(info)],
                "Select" => [CreateSelectFieldDesign(info)],
                "RadioGroup" => CreateRadioGroup(info),
                "List" => [CreateListFieldDesign(info)],
                _ => new()
            };
        }

        private static FieldDesignBase CreateFileFieldDesign(DesignInfo info)
            => new FileFieldDesign()
            {
                Name = info.FieldName,
                DbColumnFileGuid = info.Args.GetOrDefault(0),
                DbColumnFileName = info.Args.GetOrDefault(1),
                DbColumnFileSize = info.Args.GetOrDefault(2),
            };

        private static LinkFieldDesign CreateLinkFieldDesign(DesignInfo info)
            => new LinkFieldDesign
            {
                Name = info.FieldName,
                DbColumn = info.Args.GetOrDefault(0),
                SearchCondition =
                {
                    ModuleName = info.Args.GetOrDefault(1)
                },
                ValueVariable = info.Args.GetOrDefault(2),
                DisplayTextVariable = info.Args.GetOrDefault(3)
            };

        private static SelectFieldDesign CreateSelectFieldDesign(DesignInfo info)
        {
            var field = new SelectFieldDesign()
            {
                Name = info.FieldName,
                DbColumn = info.Args.GetOrDefault(0)
            };
            if (info.Args.GetOrDefault(1) == "$Candidate")
            {
                field.Candidates = info.Args.Skip(2).ToList();
            }
            else
            {
                field.SearchCondition.ModuleName = info.Args.GetOrDefault(1);
                field.ValueVariable = info.Args.GetOrDefault(2);
                field.DisplayTextVariable = info.Args.GetOrDefault(3);
            }

            return field;
        }

        private static ListFieldDesign CreateListFieldDesign(DesignInfo info)
        {
            var field = new ListFieldDesign
            {
                CanCreate = true,
                CanDelete = true,
                CanUpdate = true,
                Name = info.FieldName,
                SearchCondition =
                {
                    ModuleName = info.Args.GetOrDefault(0)
                }
            };

            var condition = info.Args.GetOrDefault(1).Split("=");
            if (condition.Length >= 2)
            {
                field.SearchCondition.Condition = new MultiMatchCondition()
                {
                    Children =
                    [
                        new FieldMatchCondition
                        {
                            FieldName = info.FieldName,
                            Children =
                            [
                                new FieldVariableMatchCondition
                                {
                                    SearchTargetVariable = condition[0].Trim(),
                                    Variable = condition[1].Trim(),
                                    Comparison = MatchComparison.Equal
                                }
                            ]
                        }
                    ]
                };
            }

            return field;
        }

        private static List<FieldDesignBase> CreateRadioGroup(DesignInfo info)
        {
            var fieldDesigns = info.Args.Select((s, i) =>
            {
                var parameters = s.Split(',');
                var text = parameters.FirstOrDefault() ?? string.Empty;
                var val = parameters.LastOrDefault() ?? string.Empty;

                return new RadioButtonFieldDesign()
                {
                    Name = info.FieldName + "Item" + i,
                    GroupField = info.FieldName,
                    Text = parameters.GetOrDefault(0),
                    Value = parameters.GetOrDefault(1)
                };
            }).OfType<FieldDesignBase>().ToList();
            fieldDesigns.Add(new RadioGroupFieldDesign() { Name = info.FieldName, DbColumn = info.Args.GetOrDefault(0) });
            return fieldDesigns;
        }
    }
}
