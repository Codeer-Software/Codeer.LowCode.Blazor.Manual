using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;

namespace LowCodeSamples.Client.Shared.Fields
{
    [ToolboxIcon(PackIconMaterialKind = "ProgressStar")]
    public class MeterFieldDesign() : FieldDesignBase(typeof(MeterFieldDesign).FullName!)
    {
        public override string GetWebComponentTypeFullName() => typeof(MeterFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override FieldBase CreateField() => new MeterField(this);
        public override FieldDataBase? CreateData() => null;
    }
}
