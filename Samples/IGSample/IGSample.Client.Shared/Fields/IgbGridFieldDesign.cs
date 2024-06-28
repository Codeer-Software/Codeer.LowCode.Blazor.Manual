using Codeer.LowCode.Blazor.Components.Fields;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Design;

namespace IGSample.Client.Shared.Fields
{
    public class IgbGridFieldDesign : ListFieldDesignBase
    {
        public IgbGridFieldDesign() : base(typeof(IgbGridFieldDesign).FullName!) { }

        [Designer(Scope = DesignerScope.All), DefaultValue(Value = true, Scope = DesignerScope.ListPage)]
        public bool CanNavigateToDetail { get; set; }

        [Designer]
        public string Height { get; set; } = "500px";

        public override string GetWebComponentTypeFullName() => typeof(IgbGridFieldComponent).FullName!;

        public override string GetSearchWebComponentTypeFullName() => string.Empty;

        public override string GetSearchControlTypeFullName() => string.Empty;

        public override FieldBase CreateField() => new ListField(this);

        public override FieldDataBase? CreateData() => new ListFieldData();
    }
}
