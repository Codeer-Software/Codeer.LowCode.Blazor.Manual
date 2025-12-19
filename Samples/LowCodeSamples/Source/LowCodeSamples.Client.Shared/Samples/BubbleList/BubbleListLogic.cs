using Codeer.LowCode.Blazor.DataIO;
using LowCodeSamples.Client.Shared.Mapping.Data;

namespace LowCodeSamples.Client.Shared.Samples.BubbleList
{
    public static class BubbleListLogic
    {
        public static async Task<List<q_売上集計>> GetSalesSummaryData(this Codeer.LowCode.Blazor.RequestInterfaces.Services services, DateOnly start, DateOnly end)
        {
            var list = (await services.ModuleDataService.GetListAsync<q_売上集計>
                (e => e.期間開始日!.Value == start && e.期間終了日!.Value == end)).ToList();
            return list ?? new();
        }
    }
}
