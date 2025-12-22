using Codeer.LowCode.Blazor.Repository.Data;
namespace LowCodeSamples.Client.Shared.Mapping.Data
{
    public class q_売上集計
    {
        public TextFieldData? 商品コード { get; set; }
        public TextFieldData? 商品名 { get; set; }
        public NumberFieldData? 合計金額 { get; set; }
        public DateFieldData? 期間開始日 { get; set; }
        public DateFieldData? 期間終了日 { get; set; }
    }
}
