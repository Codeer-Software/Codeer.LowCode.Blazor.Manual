using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Design.Samples.MobileSensor;

namespace LowCodeSamples.Client.Shared.Samples.MobileSensor
{
    public class MobileSensorField(MobileSensorFieldDesign design)
        : FieldBase<MobileSensorFieldDesign>(design)
    {
        private readonly MobileSensorFieldDesign _design = design;

        public override bool IsModified => false;

        public override FieldDataBase? GetData() => null;

        public override FieldSubmitData GetSubmitData() => new();

        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        public override bool ValidateInput() => true;
    }
}
