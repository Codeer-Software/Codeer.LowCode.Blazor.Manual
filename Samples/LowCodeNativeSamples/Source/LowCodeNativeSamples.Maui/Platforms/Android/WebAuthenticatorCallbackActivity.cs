using Android.App;
using Android.Content;
using Android.Content.PM;

namespace LowCodeNativeSamples.Maui
{
    //外部 IdP ログインの最後にシステムブラウザから戻るリダイレクトを受け取る (WebAuthenticator)。
    //スキームは ServerSettings.LoginCallbackUrl (lowcodeapp://auth) とサーバーの MobileLoginCallbackUrl に合わせる
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "lowcodeapp")]
    public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}
