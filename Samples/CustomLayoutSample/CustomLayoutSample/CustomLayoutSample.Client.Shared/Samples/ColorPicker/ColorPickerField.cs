using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;
using Design.Samples.ColorPicker;

namespace CustomLayoutSample.Client.Shared.Samples.ColorPicker
{
    public class ColorPickerField : ValueFieldBase<ColorPickerFieldDesign, ColorPickerFieldData, string>
    {
        ColorPickerFieldDesign _design;
        public ColorPickerField(ColorPickerFieldDesign design) : base(design) => _design = design;

        [ScriptHide]
        public override bool ValidateInput() => true;
    }
}
