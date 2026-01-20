using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
namespace LowCodeSamples.Client.Shared.Samples.BubbleList
{
    public class BubbleListFieldDesign() : FieldDesignBase(typeof(BubbleListFieldDesign).FullName!)
    {
        public override string GetWebComponentTypeFullName() => typeof(BubbleListFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => String.Empty;
        public override string GetSearchControlTypeFullName() => String.Empty;
        public override FieldBase CreateField() => new BubbleListField(this);
        public override FieldDataBase? CreateData() => null;
    }
}
