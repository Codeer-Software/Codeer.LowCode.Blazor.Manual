using Foundation;
using UIKit;

namespace LowCodeNativeSamples.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        //外部 IdP ログインのコールバック (lowcodeapp://auth、Info.plist の CFBundleURLTypes) を WebAuthenticator に渡す
        public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
            => Platform.OpenUrl(application, url, options) || base.OpenUrl(application, url, options);

        public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
            => Platform.ContinueUserActivity(application, userActivity, completionHandler) || base.ContinueUserActivity(application, userActivity, completionHandler);
    }
}
