using Codeer.LowCode.Blazor.Repository.Data;

namespace IGSample.Client.Shared.Samples.ColorPicker
{
  public class ColorPickerFieldData : ValueFieldDataBase<string>
  {
    public ColorPickerFieldData() : base(typeof(ColorPickerFieldData).FullName!) { }
  }
}
