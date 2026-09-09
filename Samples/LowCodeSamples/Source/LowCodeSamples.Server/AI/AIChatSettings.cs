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
    }
}
