using Codeer.LowCode.Blazor.RequestInterfaces;

namespace LowCodeApp.WinForms.LowCodeApp.Services
{
    public static class ServicesExtensions
    {
        public static AppInfoService AsImplement(this IAppInfoService src) => (AppInfoService)src;
    }
}
