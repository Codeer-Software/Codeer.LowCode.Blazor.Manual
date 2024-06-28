using Codeer.LowCode.Blazor.RequestInterfaces;

namespace LowCodeSamples.Client.Shared.Services
{
    public static class ServicesExtensions
  {
    public static AppInfoService AsImplement(this IAppInfoService src) => (AppInfoService)src;
    public static NavigationServiceBase AsImplement(this INavigationService src) => (NavigationServiceBase)src;
  }
}
