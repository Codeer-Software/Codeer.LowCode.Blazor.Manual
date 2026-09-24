using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat.RawDataAccess;
using LowCodeNativeSamples.Server.Services;
using System.Collections.Concurrent;

namespace LowCodeNativeSamples.Server.AI
{
    /// <summary>
    /// AIChatField の Agent 名 → Agent の対応表 (メールの MailSenderTable と同じ位置づけ。アプリの持ち物)。
    /// AIChatField のデザインの Agent にここの名前を書く。自分の Agent (<see cref="IAIChatAgent"/> 実装) を足すときは switch に 1 行足す。
    ///   "" / "RawDataAccess" = RawDataAccessAgent (アプリの DB を直接読んで集計・グラフで答える)。AISettings (Azure OpenAI) が設定されているときだけ使える
    /// Agent は会話履歴を持つので、名前ごとに 1 つ作って使い回す。<see cref="Service"/> がその表を使う AIChat のサーバー側入口 (プロセスに 1 つ)。
    /// null を返すと「その名前は対応表に無い」エラーになる (黙って別の Agent で答えない)。
    /// </summary>
    public static class AIChatAgentTable
    {
        static readonly ConcurrentDictionary<string, Lazy<IAIChatAgent?>> _agents = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>AIChatController が使う入口。送信で Agent をバックグラウンド実行し、ポーリングに状態を返す。</summary>
        public static AIChatService Service { get; } = new(Create);

        /// <summary>名前に対応する Agent (無ければ null = ジョブは error)。</summary>
        public static IAIChatAgent? Create(string name)
            => _agents.GetOrAdd(name ?? string.Empty, n => new Lazy<IAIChatAgent?>(() => CreateCore(n))).Value;

        static IAIChatAgent? CreateCore(string name) => name switch
        {
            "" => CreateRawDataAccess(),
            "RawDataAccess" => CreateRawDataAccess(),
            _ => null,
        };

        //読むデータソースは appsettings の AIChat:RawDataAccessDataSources (空なら DataSources の全部)。本番では AI 用の読み取り専用 DB ユーザーで接続するデータソースを指す (何が読めるかは DB 側の権限で決める)。
        //設計 (モジュール定義) と、フィールドの DocumentFolder が指すデザインプロジェクトの Resources/{folder}/*.md|*.txt (業務用語や集計の決まり) も渡す
        static IAIChatAgent? CreateRawDataAccess()
        {
            var config = SystemConfig.Instance;
            //IChatClient は Extras.Server の AzureOpenAIClients が AISettings (Azure OpenAI) から作る。別プロバイダ (OpenAI / Ollama …) ならここで自分で作って渡す。設定が欠けていれば null = AI Agent は使えない
            var chatClientFactory = AzureOpenAIClients.ChatClientFactory(config.AISettings);
            if (chatClientFactory == null) return null;
            var dataSourceNames = config.AIChat.RawDataAccessDataSources.Length == 0
                ? config.DataSources.Select(e => e.Name).ToList()
                : config.AIChat.RawDataAccessDataSources.ToList();
            return new RawDataAccessAgent(
                chatClientFactory,
                () => new DbAccessor(config.DataSources),
                () => DesignerService.GetDesignData(),
                folder => DesignDataFileManager.GetResourceTexts(config.DesignFileDirectory, folder, ".md", ".txt").Select(e => new AIChatDocument(e.Name, e.Text)).ToList(),
                new RawDataAccessOptions { DataSourceNames = dataSourceNames },
                //SemanticSearchField を置いたモジュールを search_records (意味検索) で探せるようにする (埋め込みプロバイダ未設定ならツールは付かない)
                semanticSearch: SemanticSearchIndex.Service);
        }
    }
}
