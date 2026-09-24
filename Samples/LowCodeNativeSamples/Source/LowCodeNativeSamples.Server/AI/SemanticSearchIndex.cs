using Codeer.LowCode.Blazor.Extras.Server.AI.Embedding;
using Codeer.LowCode.Blazor.Extras.Server.AI.SemanticSearch;
using LowCodeNativeSamples.Server.Services;

namespace LowCodeNativeSamples.Server.AI
{
    /// <summary>
    /// SemanticSearchField (意味検索) のサーバー側の持ち物 = <see cref="SemanticSearchService"/> をプロセスに 1 つ。
    /// 埋め込みプロバイダは appsettings の SemanticSearch.EmbeddingProvider の呼び名で <see cref="EmbeddingProviderTable"/> から選ぶ (Azure OpenAI / 独自)。
    /// 呼び名が空 (対応表に無い) なら埋め込み無し = 文章だけ保存され、意味検索ツールは付かない。
    /// 使う場所: CustomizedModuleDataIO (保存時の索引付け) / SemanticSearchController (再索引 API) / AIChatAgentTable (RawDataAccessAgent の search_records)。
    /// </summary>
    internal static class SemanticSearchIndex
    {
        static readonly Lazy<IEmbeddingProvider?> _provider = new(() => EmbeddingProviderTable.Create(SystemConfig.Instance.SemanticSearch.EmbeddingProvider));

        public static SemanticSearchService Service { get; } = new(() => _provider.Value, () => DesignerService.GetDesignData());
    }
}
