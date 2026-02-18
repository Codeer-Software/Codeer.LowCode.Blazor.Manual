using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;
using Design.Samples.ColorPicker;

namespace CodeerLowCodeBlazorTemplate.Client.Shared.Samples.ColorPicker
{
    public class ColorPickerField(ColorPickerFieldDesign design)
        : ValueFieldBase<ColorPickerFieldDesign, ColorPickerFieldData, string>(design)
    {
        private readonly ColorPickerFieldDesign _design = design;

        [ScriptHide]
        public override bool ValidateInput() => true;
    }
}
