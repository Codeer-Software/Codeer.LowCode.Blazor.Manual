using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;
using LowCodeSamples.Client.Shared.Mapping.Data;

namespace LowCodeSamples.Client.Shared.Samples.BubbleList
{
    public class BubbleListField : FieldBase<BubbleListFieldDesign>
    {
        public BubbleListField(BubbleListFieldDesign design) : base(design) { }

        [ScriptHide]
        public override bool IsModified => false;
        [ScriptHide]
        public override FieldDataBase? GetData() => null;
        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();
        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;
        [ScriptHide]
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        [ScriptHide]
        public DateOnly? Start { get; set; }
        [ScriptHide]
        public DateOnly? End { get; set; }
        [ScriptHide]
        private bool IsValidRange => Start <= End;
        [ScriptHide]
        public List<q_売上集計> SalesSummaryData { get; set; } = new();
        [ScriptHide]
        public string SelectedSort { get; set; } = "amount-desc";

        public async Task DateCange(DateOnly? start, DateOnly? end)
        {
            Start = start;
            End = end;
            await ReloadAsync();
        }

        public async Task SortChange(string sort)
        {
            if (sort == "low")
            {
                SelectedSort = "amount-asc";
            }
            else
            {
                SelectedSort = "amount-desc";
            }

            await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            if (!IsValidRange)
            {
                SalesSummaryData = new();
                return;
            }

            SalesSummaryData = await Services.ModuleDataService.GetListAsync<q_売上集計>
                                        (e => e.期間開始日!.Value == Start && e.期間終了日!.Value == End);
            NotifyStateChanged();
        }
    }
}
