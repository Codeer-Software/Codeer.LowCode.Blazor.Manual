using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using CustomLayoutSample.Client.Shared.Samples.ColorPicker;

//Namespace fixed for consistency with sample designer project
namespace Design.Samples.ColorPicker
{
    public class ColorPickerFieldDesign : ValueFieldDesignBase
    {
        public ColorPickerFieldDesign() : base(typeof(ColorPickerFieldDesign).FullName!) { }

        [Designer(Index = 0, CandidateType = CandidateType.DbColumn), DbColumn(nameof(ColorPickerFieldData.Value))]
        public string DbColumn { get; set; } = string.Empty;

        [Designer(Index = 1)]
        public string Default { get; set; } = "#000000";

        public override string GetWebComponentTypeFullName() => typeof(ColorPickerFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => String.Empty;
        public override string GetSearchControlTypeFullName() => String.Empty;
        public override FieldBase CreateField() => new ColorPickerField(this);
        public override FieldDataBase? CreateData() => new ColorPickerFieldData();
    }
}
