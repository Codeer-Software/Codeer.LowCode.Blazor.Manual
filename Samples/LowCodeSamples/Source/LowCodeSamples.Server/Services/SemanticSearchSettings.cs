namespace LowCodeSamples.Server.Services
{
    /// <summary>意味検索 (SemanticSearchField) のアプリ設定 (appsettings の "SemanticSearch")。</summary>
    public class SemanticSearchSettings
    {
        /// <summary>埋め込みプロバイダの呼び名 (EmbeddingProviderTable の鍵: "AzureOpenAI" / 独自)。空なら意味検索なし。</summary>
        public string EmbeddingProvider { get; set; } = string.Empty;
    }
}
