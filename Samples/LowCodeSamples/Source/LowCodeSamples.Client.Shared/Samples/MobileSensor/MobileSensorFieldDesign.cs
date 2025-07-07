using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using LowCodeSamples.Client.Shared.Samples.MobileSensor;

namespace Design.Samples.MobileSensor
{
    [ToolboxIcon(ResourcePath = "Resources/mobilesensor.png")]
    public class MobileSensorFieldDesign() : ValueFieldDesignBase(typeof(MobileSensorFieldDesign).FullName!)
    {
        public override string GetWebComponentTypeFullName() => typeof(MobileSensorFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => String.Empty;
        public override string GetSearchControlTypeFullName() => String.Empty;
        public override FieldBase CreateField() => new MobileSensorField(this);
        public override FieldDataBase? CreateData() => null;
    }
}
