using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat.RawDataAccess;
using LowCodeSamples.Server.Services;
using System.Collections.Concurrent;

namespace LowCodeSamples.Server.AI
{
    /// <summary>
    /// AIChatField の Agent 名 → Agent の対応表 (メールの MailSenderTable と同じ位置づけ。アプリの持ち物)。
    /// AIChatField のデザインの Agent にここの名前を書く。自分の Agent (<see cref="IAIChatAgent"/> 実装) を足すときは switch に 1 行足す。
    ///   "" / "RawDataAccess" = RawDataAccessAgent (DB を直接読んで集計・グラフで答える)。AISettings (Azure OpenAI) が設定されているときだけ使える
    /// Agent は会話履歴を持つので、名前ごとに 1 つ作って使い回す。<see cref="Service"/> がその表を使う AIChat のサーバー側入口 (プロセスに 1 つ)。
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

        //読むデータソースは appsettings の AIChat:RawDataAccessDataSources。デモサイトでは読み取り専用の DB ユーザー (ai_chat_reader) で接続する
        //データソース "AIChat" を指し、AI が読める表は DB 側の GRANT で売上デモの 3 表だけにしている (何が読めるかはライブラリでなく DB 側の権限で決める)。
        //設計 (モジュール定義) と、フィールドの DocumentFolder が指すデザインプロジェクトの Resources/{folder}/*.md|*.txt (業務用語や集計の決まり) も渡す
        static IAIChatAgent? CreateRawDataAccess()
        {
            var config = SystemConfig.Instance;
            //IChatClient は Extras.Server の AzureOpenAIClients が AISettings (Azure OpenAI) から作る。別プロバイダ (OpenAI / Ollama …) ならここで自分で作って渡す。設定が欠けていれば null = AI Agent は使えない
            var chatClientFactory = AzureOpenAIClients.ChatClientFactory(config.AISettings);
            if (chatClientFactory == null) return null;
            return new RawDataAccessAgent(
                chatClientFactory,
                () => new DbAccessor(config.DataSources),
                () => DesignerService.GetDesignData(),
                folder => DesignDataFileManager.GetResourceTexts(config.DesignFileDirectory, folder, ".md", ".txt").Select(e => new AIChatDocument(e.Name, e.Text)).ToList(),
                new RawDataAccessOptions { DataSourceNames = config.AIChat.RawDataAccessDataSources },
                //SemanticSearchField を置いたモジュールを search_records (意味検索) で探せるようにする (埋め込みプロバイダ未設定ならツールは付かない)
                semanticSearch: SemanticSearchIndex.Service);
        }
    }
}
