using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace LowCodeSamples.Client.Shared.Services
{
    public abstract class NavigationServiceBase : INavigationService
    {
        readonly NavigationManager _navigationManager;
        readonly IAppInfoService _appInfo;

        public abstract bool CanLogout { get; }

        public abstract Task Logout();

        public NavigationServiceBase(NavigationManager navigationManager, IAppInfoService appInfo)
        {
            _navigationManager = navigationManager;
            _appInfo = appInfo;
        }

        public string GetModuleUrl(string module) => $"/{GetCurrentPageFrame()}/{module}";

        public string GetModuleUrl(string pageFrame, string module) => $"/{pageFrame}/{module}";

        public string GetModuleDataUrl(string module, string id) => $"/{GetCurrentPageFrame()}/{module}/{id}";

        public string GetModuleDataUrl(string pageFrame, string module, string id) => $"/{pageFrame}/{module}/{id}";

        public void NavigateTo(string url) => _navigationManager.NavigateTo(url);

        public void ReplaceTo(string url) => _navigationManager.NavigateTo(url, false, true);

        public Dictionary<string, List<string>> GetQueryParameters()
            => QueryHelpers.ParseQuery(new Uri(_navigationManager.Uri).Query).ToDictionary(e => e.Key, e => e.Value.Select(e => e ?? string.Empty).ToList());

        public string GetCurrentPageFrame()
            => _navigationManager.Uri.Substring(_navigationManager.BaseUri.Length).Split('/').FirstOrDefault() ?? string.Empty;
  }

  public class PageLinkUrlResolver : IPageLinkUrlResolver
  {
    public string GetModuleUrl(string currentPageFrame, PageLink pageLink)
        => string.IsNullOrEmpty(pageLink.PageFrame) ?
            $"/{currentPageFrame}/{pageLink.Module}" :
            $"/{pageLink.PageFrame}/{pageLink.Module}";
  }
}
