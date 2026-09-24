using Codeer.LowCode.Blazor.Extras.Server.AI.Embedding;
using LowCodeSamples.Server.Services;

namespace LowCodeSamples.Server.AI
{
    /// <summary>
    /// 「埋め込みプロバイダの呼び名」→ 実装 (<see cref="IEmbeddingProvider"/>) の対応表 (メールの MailSenderTable と同じ位置づけ。アプリの持ち物)。
    /// 呼び名は appsettings の SemanticSearch.EmbeddingProvider で指定する (索引と検索は同じモデルでないと成立しないのでアプリ単位)。
    /// </summary>
    /// <remarks>
    /// プロバイダごとの設定は appsettings の独立したセクション ("AzureOpenAIEmbedding") で、Program.cs が個別に読んでいる。
    /// 別のプロバイダ (OpenAI / Ollama などのローカルモデル・社内 API 等) を使うときは <see cref="IEmbeddingProvider"/> を実装してこの switch に 1 行足す
    /// (Microsoft.Extensions.AI の IEmbeddingGenerator を持っているなら EmbeddingGeneratorProvider で包む)。
    /// null を返すと「その呼び名は対応表に無い」= 意味検索なし (文章だけ保存・ツール無し)。
    /// モデルを変えたら DB のベクトル列の次元を合わせて作り直し、SemanticSearchField のスクリプト Reindex で全行を再索引する。
    /// </remarks>
    public static class EmbeddingProviderTable
    {
        public static IEmbeddingProvider? Create(string name)
        {
            var config = SystemConfig.Instance;
            return name switch
            {
                "AzureOpenAI" => new AzureOpenAIEmbeddingProvider(config.AzureOpenAIEmbedding),
                _ => null,
            };
        }
    }
}
