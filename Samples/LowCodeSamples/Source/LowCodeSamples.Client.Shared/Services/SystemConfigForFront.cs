namespace LowCodeSamples.Client.Shared.Services
{
    public class SystemConfigForFront
    {
        public bool CanScriptDebug { get; set; }
        public bool UseHotReload { get; set; }
        //デモサイトの固定操作ユーザー (AppUser の Id)。認証を持たないため、サーバーが決めた値をクライアントも使う
        public string CurrentUserId { get; set; } = string.Empty;
    }
}
