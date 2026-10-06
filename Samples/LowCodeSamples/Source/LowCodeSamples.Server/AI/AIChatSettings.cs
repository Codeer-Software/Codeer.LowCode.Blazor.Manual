using Codeer.LowCode.Blazor.Extras.Server.AI.Chat.ModuleDataAccess;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat.RawDataAccess;

namespace LowCodeSamples.Server.AI
{
    /// <summary>appsettings の "AIChat" セクション (AIChatField のサーバー側の設定。アプリの持ち物)。</summary>
    public class AIChatSettings
    {
        /// <summary>
        /// RawDataAccess Agent が読むデータソース名 (appsettings の DataSources の Name。複数可)。
        /// デモサイトは読み取り専用の DB ユーザーで接続するデータソース "AIChat" (見せてよい表だけ SELECT を GRANT)。
        /// 何が読めるかはライブラリではなく DB 側の権限で決める。
        /// </summary>
        public string[] RawDataAccessDataSources { get; set; } = [];

        /// <summary>RawDataAccess Agent の上限値 (SQL のタイムアウト・1 返事の合計時間・同時実行数など。0 / false で無効。待たせる・断る種類のものは既定で無効)。</summary>
        public RawDataAccessOptions RawDataAccess { get; set; } = new();

        /// <summary>ModuleDataAccess Agent の上限値 (SQL のタイムアウト・1 返事の合計時間・同時実行数など。0 で無効)。</summary>
        public ModuleDataAccessOptions ModuleDataAccess { get; set; } = new();
    }
}
