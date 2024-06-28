using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace CustomLayoutSample.Client.Shared.Services
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

        public string GetTopPageUrl()
        {
            var mainLayout = _appInfo.GetDesignData().GetMainPageFrameDesign();
            if (mainLayout == null) return string.Empty;
            return GetModuleUrl(mainLayout.Name, mainLayout.TopPageModule);
        }

        public string GetUrl(PageLink pageLink)
        {
            var pageFrame = string.IsNullOrEmpty(pageLink.PageFrame) ? GetCurrentPageFrame() : pageLink.PageFrame;
            var url = $"/{pageFrame}/{pageLink.Module}";
            var search = _appInfo.GetDesignData().GetDefaultListPageSearchParameter(pageLink.Module);
            if (!string.IsNullOrEmpty(search)) return url + "?" + search;
            return url;
        }

        public string GetModuleUrl(string module) => $"/{GetCurrentPageFrame()}/{module}";

        public string GetModuleUrl(string pageFrame, string module) => $"/{pageFrame}/{module}";

        public string GetModuleDataUrl(string module, string id) => $"/{GetCurrentPageFrame()}/{module}/{id}";

        public string GetModuleDataUrl(string pageFrame, string module, string id) => $"/{pageFrame}/{module}/{id}";

        public void NavigateTo(string url) => _navigationManager.NavigateTo(url);

        public void ReplaceTo(string url) => _navigationManager.NavigateTo(url, false, true);

        public Dictionary<string, List<string>> GetQueryParameters()
            => QueryHelpers.ParseQuery(new Uri(_navigationManager.Uri).Query).ToDictionary(e => e.Key, e => e.Value.Select(e => e ?? string.Empty).ToList());

        string GetCurrentPageFrame()
            => _navigationManager.Uri.Substring(_navigationManager.BaseUri.Length).Split('/').FirstOrDefault() ?? string.Empty;
    }
}
