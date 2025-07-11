﻿using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using WebApp.Client.Shared.Samples.ColorPicker;

//Namespace fixed for consistency with sample designer project
namespace Design.Samples.ColorPicker
{
    [ToolboxIcon(ResourcePath = "Resources/colorpicker.png")]
    public class ColorPickerFieldDesign() : ValueFieldDesignBase(typeof(ColorPickerFieldDesign).FullName!)
    {
        [Designer(Index = 0, CandidateType = CandidateType.DbColumn), DbColumn(nameof(ColorPickerFieldData.Value))]
        public string DbColumn { get; set; } = string.Empty;

        [Designer(Index = 1, CandidateType = CandidateType.Color)]
        public string Default { get; set; } = "#000000";

        public override string GetWebComponentTypeFullName() => typeof(ColorPickerFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => String.Empty;
        public override string GetSearchControlTypeFullName() => String.Empty;
        public override FieldBase CreateField() => new ColorPickerField(this);
        public override FieldDataBase? CreateData() => new ColorPickerFieldData();

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = new List<DesignCheckInfo>();
            context.CheckFieldName(Name).AddTo(result);
            context.CheckFieldDbColumnExistence(Name, nameof(DbColumn), DbColumn).AddTo(result);
            context.CheckFieldFunctionExistence(Name, nameof(OnDataChanged), OnDataChanged,
                context.GetScriptMethodAttribute(GetType(), nameof(OnDataChanged))).AddTo(result);
            return result;
        }
    }
}
