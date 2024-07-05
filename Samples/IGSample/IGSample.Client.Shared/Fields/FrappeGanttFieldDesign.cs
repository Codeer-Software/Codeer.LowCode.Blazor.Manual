using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;

namespace IGSample.Client.Shared.Fields
{
    public class FrappeGanttFieldDesign() : ListFieldDesignBase(typeof(FrappeGanttFieldDesign).FullName!)
    {
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string IdField { get; set; } = "";
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string NameField { get; set; }= "";
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string StartDateField { get; set; }= "";
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string EndDateField { get; set; }= "";
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string ProgressField { get; set; }= "";
        [Designer(CandidateType = CandidateType.Variable)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string DependenciesField { get; set; }= "";

        public override string GetWebComponentTypeFullName() => typeof(FrappeGanttFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override FieldDataBase? CreateData() => null;
        public override FieldBase CreateField() => new ListField(this);
    }
}
