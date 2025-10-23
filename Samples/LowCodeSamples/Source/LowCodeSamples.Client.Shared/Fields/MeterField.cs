using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;

namespace LowCodeSamples.Client.Shared.Fields
{
    public class MeterField(MeterFieldDesign design)
        : FieldBase<MeterFieldDesign>(design)
    {
        public double Value { get; private set; }

        public override bool IsModified => false;
        public override FieldDataBase? GetData() => null;
        public override FieldSubmitData GetSubmitData() => new();
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        [ScriptMethodToProperty("Value")]
        public void SetValue(double value)
        {
            if (Value == value) return;
            Value = value;
            NotifyStateChanged();
        }
    }
}
