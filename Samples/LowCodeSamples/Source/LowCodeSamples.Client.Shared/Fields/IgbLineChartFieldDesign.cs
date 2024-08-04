using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;

namespace LowCodeSamples.Client.Shared.Fields
{
    public class IgbLineChartFieldDesign : ListFieldDesignBase
    {
        public IgbLineChartFieldDesign() : base(typeof(IgbLineChartFieldDesign).FullName!) { }

        [Designer]
        public string[] Keys { get; set; } = [];

        [Designer]
        public string XAxisTitle { get; set; } = "";

        [Designer]
        public string YAxisTitle { get; set; } = "";

        [Designer]
        public string Height { get; set; } = "500px";
        
        public override string GetWebComponentTypeFullName() => typeof(IgbLineChartFieldComponent).FullName!;

        public override string GetSearchWebComponentTypeFullName() => string.Empty;

        public override string GetSearchControlTypeFullName() => string.Empty;

        public override FieldBase CreateField() => new ListField(this);

        public override FieldDataBase? CreateData() => null;
    
        public override string LayoutName { get; set; } = string.Empty;
  }
}
