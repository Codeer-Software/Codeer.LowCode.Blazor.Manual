using AccessSample.Client.Shared.AITextAnalyzer;
using AccessSample.Client.Shared.Samples.AIDocumentAnalyzer;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;

//Namespace fixed for consistency with sample designer project
namespace Design.Samples.AIDocumentAnalyzer
{
    [ToolboxIcon(PackIconMaterialKind = "HeadSnowflakeOutline")]
    public class AITextAnalyzerFieldDesign() : FieldDesignBase(typeof(AITextAnalyzerFieldDesign).FullName!)
    {
        [Designer(CandidateType = CandidateType.ScriptEvent)]
        public string DataImportCompleted { get; set; } = string.Empty;

        public override string GetWebComponentTypeFullName() => typeof(AITextAnalyzerFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override FieldBase CreateField() => new AITextAnalyzerField(this);
        public override FieldDataBase? CreateData() => null;
    }
}
