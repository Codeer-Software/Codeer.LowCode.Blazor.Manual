namespace LowCodeNativeSamples.Maui.Services
{
    /// <summary>
    /// Server URL setting. Stored with MAUI Preferences (plain app storage, outside Blazor) so it can be
    /// changed from the native SettingsPage. The default comes from appsettings.json.
    /// </summary>
    public static class ServerSettings
    {
        const string BaseUrlKey = "Server.BaseUrl";

        /// <summary>Set once at startup from appsettings.json (Server:BaseUrl).</summary>
        public static string DefaultBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// 外部 IdP ログイン後にシステムブラウザから戻ってくる URL (Server:LoginCallbackUrl、既定 lowcodeapp://auth)。
        /// スキームは Android の WebAuthenticatorCallbackActivity / iOS の Info.plist CFBundleURLTypes、
        /// およびサーバー側の MobileLoginCallbackUrl と一致させる。
        /// </summary>
        public static string LoginCallbackUrl { get; set; } = "lowcodeapp://auth";

        public static string BaseUrl
        {
            get => Preferences.Default.Get(BaseUrlKey, string.Empty) is { Length: > 0 } saved ? saved : DefaultBaseUrl;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == DefaultBaseUrl) Preferences.Default.Remove(BaseUrlKey);
                else Preferences.Default.Set(BaseUrlKey, value);
            }
        }

        public static bool IsValidUrl(string? text)
            => Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
