using Azure;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat.RawDataAccess;
using LowCodeSamples.Server.Services;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace LowCodeSamples.Server.AI
{
    /// <summary>
    /// AIChatField の Agent 名 → Agent の対応表 (メールの MailSenderTable と同じ位置づけ。アプリの持ち物)。
    /// AIChatField のデザインの Agent にここの名前を書く。自分の Agent (<see cref="IAIChatAgent"/> 実装) を足すときは switch に 1 行足す。
    ///   "" / "RawDataAccess" = RawDataAccessAgent (DB を直接読んで集計・グラフで答える)。AISettings (Azure OpenAI) が設定されているときだけ使える
    /// Agent は会話履歴を持つので、名前ごとに 1 つ作って使い回す。<see cref="Jobs"/> がその表を使うジョブ置き場 (プロセスに 1 つ)。
    /// </summary>
    public static class AIChatAgentTable
    {
        static readonly ConcurrentDictionary<string, Lazy<IAIChatAgent?>> _agents = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>AIChatController が使うジョブ置き場。送信で Agent をバックグラウンド実行し、ポーリングに状態を返す。</summary>
        public static AIChatJobStore Jobs { get; } = new(Create);

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
            var chatClientFactory = CreateAzureOpenAI(config.AISettings);
            if (chatClientFactory == null) return null;
            return new RawDataAccessAgent(
                chatClientFactory,
                () => new DbAccessor(config.DataSources),
                () => DesignerService.GetDesignData(),
                folder => DesignDataFileManager.GetResourceTexts(config.DesignFileDirectory, folder, ".md", ".txt").Select(e => new AIChatDocument(e.Name, e.Text)).ToList(),
                new RawDataAccessOptions { DataSourceNames = config.AIChat.RawDataAccessDataSources });
        }

        //Agent に渡す IChatClient。ライブラリは IChatClient 抽象しか知らないので、どのプロバイダ (Azure OpenAI / OpenAI / Ollama …) を使うかはここで決める。
        //AISettings の OpenAIEndPoint / OpenAIKey / ChatModel が揃っているときだけ返す (欠けていれば null = AI Agent は使えない)
        static Func<IChatClient>? CreateAzureOpenAI(AISettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.OpenAIEndPoint) || string.IsNullOrWhiteSpace(settings.OpenAIKey) || string.IsNullOrWhiteSpace(settings.ChatModel))
                return null;
            var client = new AzureOpenAIClient(new Uri(settings.OpenAIEndPoint), new AzureKeyCredential(settings.OpenAIKey));
            return () => client.GetChatClient(settings.ChatModel).AsIChatClient();
        }
    }
}
