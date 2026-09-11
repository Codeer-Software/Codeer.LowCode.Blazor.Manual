using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeNativeSamples.Client.Shared.Services;
using LowCodeNativeSamples.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Auth;

namespace LowCodeNativeSamples.Server.Services
{
    public class SystemConfig
    {
        public static SystemConfig Instance { get; set; } = new();

        public bool CanScriptDebug { get; set; }
        public bool UseHotReload { get; set; }
        public DataSource[] DataSources { get; set; } = [];
        //ファイル保存先 = 種類ごとの設定 (使うものだけ書けばよい)。実体 (IFileStorage) は Services/FileStorageTable が組み立てる (メールの MailSenderTable と同じ考え方)
        public FileSystemStorageSettings[] FileSystemStorages { get; set; } = [];
        public AzureBlobStorageSettings[] AzureBlobStorages { get; set; } = [];
        public S3StorageSettings[] S3Storages { get; set; } = [];
        //簡易形式 (種別と設定を 1 クラスに持つ。FileSystem / Azure Blob 接続文字列)
        public FileStorage[] FileStorages { get; set; } = [];
        public TemporaryFileTableInfo[] TemporaryFileTableInfo { get; set; } = [];
        public string DesignFileDirectory { get; set; } = string.Empty;
        public string FontFileDirectory { get; set; } = string.Empty;
        //Mail = 製品 (共通層) が読む設定。プロバイダごとの設定 (Smtp / Gmail 等) は個別のセクションとして持つ
        public MailConfig Mail { get; set; } = new();
        public SmtpSettings Smtp { get; set; } = new();
        public GraphApiSettings GraphApi { get; set; } = new();
        public SendGridSettings SendGrid { get; set; } = new();
        public GmailSettings Gmail { get; set; } = new();
        public AISettings AISettings { get; set; } = new();
        //AIChatField のサーバー側 (AI/AIChatAgentTable)。AI が読むデータソース等
        public AIChatSettings AIChat { get; set; } = new();
        //ID/パスワードのログイン (表・列はユーザーモジュールのデザインから: IdField / LoginAccountContractField / PasswordHashField)。外部 IdP 専用にするなら false (ログイン画面はプロバイダのボタンだけになる)
        public bool AllowPasswordLogin { get; set; } = true;
        //外部 IdP (Entra ID / Google / AWS Cognito / OIDC) = 種類ごとの設定 (使うものだけ書けばよい)。実体 (IExternalLoginProvider) は Services/ExternalLoginTable が組み立てる
        public EntraLoginSettings EntraLogin { get; set; } = new();
        public GoogleLoginSettings GoogleLogin { get; set; } = new();
        public CognitoLoginSettings CognitoLogin { get; set; } = new();
        public OidcLoginSettings[] OidcLogins { get; set; } = [];
        //MAUI アプリがシステムブラウザで外部 IdP にログインした後に戻る URL。MAUI 側の appsettings (Server:LoginCallbackUrl) と一致させる
        public string MobileLoginCallbackUrl { get; set; } = string.Empty;
        //ID/パスワードのログインに足す二要素認証 (TOTP)。有効・無効はデザイン (LoginAccountContractField の TOTP 列) で決まり、ここは表示用の Issuer だけ (docs: Codeer.LowCode.Blazor.Extras の TwoFactorLogin.md)
        public TotpLoginSettings TotpLogin { get; set; } = new();
        //メールのワンタイムコードによる二要素認証。有効・無効はデザイン (LoginAccountContractField の TwoFactorEmail) で決まり、ここはメールの体裁と有効期限だけ
        public EmailOtpLoginSettings EmailOtpLogin { get; set; } = new();
        public SystemConfigForFront ForFront() => new SystemConfigForFront { CanScriptDebug = CanScriptDebug, UseHotReload = UseHotReload };
    }
}
