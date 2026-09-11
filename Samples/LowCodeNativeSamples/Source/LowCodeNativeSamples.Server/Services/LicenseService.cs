using Codeer.LowCode.Blazor.License;

namespace LowCodeNativeSamples.Server.Services
{
    static class LicenseService
    {
        static DateTime _checkedTime;

        //ライセンス更新。専用エンドポイントは持たず、デザインデータ取得の直前に呼ぶ。
        //1分間隔にスロットリングし、更新に失敗しても既存のライセンス状態で処理を続行させる。
        internal static async Task UpdateAsync(HttpRequest request)
        {
            var now = DateTime.Now;
            if (now - _checkedTime < TimeSpan.FromMinutes(1)) return;
            _checkedTime = now;
            try
            {
                await LicenseManager.CheckServerLicense(request);
            }
            catch { }
        }
    }
}
