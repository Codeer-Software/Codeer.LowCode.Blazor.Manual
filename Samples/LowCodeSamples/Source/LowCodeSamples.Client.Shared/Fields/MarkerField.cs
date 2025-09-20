using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;

namespace LowCodeSamples.Client.Shared.Fields
{
    public class MarkerField : FieldBase<MarkerFieldDesign>
    {
        public MarkerField(MarkerFieldDesign design) : base(design) { }
        public override bool IsModified => false;
        public override FieldDataBase? GetData() => null;
        public override FieldSubmitData GetSubmitData() => new();
        public override Task InitializeDataAsync(FieldDataBase? fieldDataBase) => Task.CompletedTask;
        public override Task SetDataAsync(FieldDataBase? fieldDataBase) => Task.CompletedTask;

        internal async Task OnClickAsync(int x, int y)
        {
            var target = Module.GetField(Design.TargetFieldName);
            if (target != null) await target.FocusAsync();
            await Module!.ExecuteScriptAsync(Design.OnClick, x, y);
        }
    }
}
