using Codeer.LowCode.Blazor.Extras.Server.Auth;

namespace LowCodeNativeSamples.Server.Services
{
    /// <summary>
    /// SystemConfig の外部 IdP 設定 (種類ごと: EntraLogin / GoogleLogin / CognitoLogin / OidcLogins) → IExternalLoginProvider の対応表
    /// (メールの MailSenderTable / ファイル保存の FileStorageTable と同じ考え方)。ClientId が書かれているものだけ並べる。
    /// 独自の IdP を足すときは IExternalLoginProvider を実装 (多くは OidcLoginProvider を継承) してここに追加する。
    /// </summary>
    public static class ExternalLoginTable
    {
        public static List<IExternalLoginProvider> Create()
        {
            var config = SystemConfig.Instance;
            var list = new List<IExternalLoginProvider>();
            if (config.EntraLogin.IsConfigured) list.Add(new EntraLoginProvider(config.EntraLogin));
            if (config.GoogleLogin.IsConfigured) list.Add(new GoogleLoginProvider(config.GoogleLogin));
            if (config.CognitoLogin.IsConfigured) list.Add(new CognitoLoginProvider(config.CognitoLogin));
            //汎用 OpenID Connect (Keycloak / Auth0 / LINE 等) は複数置ける。Name が URL とボタンの識別になる
            foreach (var e in config.OidcLogins)
            {
                if (e.IsConfigured) list.Add(new OidcLoginProvider(e));
            }
            return list;
        }
    }
}
