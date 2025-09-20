using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;

namespace LowCodeSamples.Client.Shared.Fields
{
    public class MarkerFieldDesign : FieldDesignBase
    {
        public MarkerFieldDesign() : base(typeof(MarkerFieldDesign).FullName!) { }

        [Designer(Index = 7, CandidateType = CandidateType.Field)]
        public string TargetFieldName { get; set; } = string.Empty;

        [Designer(Index = 7, CandidateType = CandidateType.ScriptEvent),
         ScriptMethod(ArgumentTypes = ["int", "int"], ArgumentNames = ["x", "y"])]
        public string OnClick { get; set; } = string.Empty;

        public override FieldDataBase? CreateData() => null;
        public override FieldBase CreateField() => new MarkerField(this);
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetWebComponentTypeFullName()=>typeof(MarkerFieldComponent).FullName!;
    }
}
