namespace LowCodeNativeSamples.Server.AI
{
    /// <summary>appsettings の "AIChat" セクション (AIChatField のサーバー側の設定。アプリの持ち物)。</summary>
    public class AIChatSettings
    {
        /// <summary>
        /// RawDataAccess Agent が読むデータソース名 (appsettings の DataSources の Name。複数可)。空なら DataSources の全部。
        /// 本番では AI 用の読み取り専用 DB ユーザー (見せてよい表・列だけ SELECT を GRANT) で接続するデータソースを別に用意してここに書く。
        /// 何が読めるかはライブラリではなく DB 側の権限で決める。
        /// </summary>
        public string[] RawDataAccessDataSources { get; set; } = [];
    }
}
