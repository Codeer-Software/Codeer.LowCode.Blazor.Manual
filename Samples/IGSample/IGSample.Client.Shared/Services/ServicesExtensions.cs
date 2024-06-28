using Codeer.LowCode.Blazor.RequestInterfaces;

namespace IGSample.Client.Shared.Services
{
  public static class ServicesExtensions
  {
    public static AppInfoService AsImplement(this IAppInfoService src) => (AppInfoService)src;
  }
}
