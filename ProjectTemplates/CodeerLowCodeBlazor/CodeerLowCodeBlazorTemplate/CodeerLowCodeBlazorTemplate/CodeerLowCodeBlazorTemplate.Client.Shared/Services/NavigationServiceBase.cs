using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace CodeerLowCodeBlazorTemplate.Client.Shared.Services
{
    public abstract class NavigationServiceBase : INavigationService
    {
        readonly NavigationManager _navigationManager;

        public abstract bool CanLogout { get; }

        public abstract Task Logout();

        public NavigationServiceBase(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public string GetModuleUrl(string moduleSegment) => $"/{GetCurrentPageFrame()}/{moduleSegment}";

        public string GetModuleUrl(string pageFrameSegment, string moduleSegment) => $"/{pageFrameSegment}/{moduleSegment}";

        public string GetModuleDataUrl(string moduleSegment, string idSegment) => $"/{GetCurrentPageFrame()}/{moduleSegment}/{idSegment}";

        public string GetModuleDataUrl(string pageFrameSegment, string moduleSegment, string idSegment) => $"/{pageFrameSegment}/{moduleSegment}/{idSegment}";

        public void NavigateTo(string url) => _navigationManager.NavigateTo(url);

        public void ReplaceTo(string url) => _navigationManager.NavigateTo(url, false, true);

        public Dictionary<string, List<string>> GetQueryParameters()
            => QueryHelpers.ParseQuery(new Uri(_navigationManager.Uri).Query).ToDictionary(e => e.Key, e => e.Value.Select(e => e ?? string.Empty).ToList());

        string GetCurrentPageFrame()
            => _navigationManager.Uri.Substring(_navigationManager.BaseUri.Length).Split('/').FirstOrDefault() ?? string.Empty;
    }

    public class PageLinkUrlResolver : IPageLinkUrlResolver
    {
        public string GetModuleUrl(string currentPageFrame, PageLink pageLink)
        {
            var moduleUrlSegment = string.IsNullOrEmpty(pageLink.ModuleUrlSegment) ? pageLink.Module : pageLink.ModuleUrlSegment;
            return string.IsNullOrEmpty(pageLink.PageFrame) ?
                $"/{currentPageFrame}/{moduleUrlSegment}/{pageLink.Id}".Trim('/') :
                $"/{pageLink.PageFrame}/{moduleUrlSegment}/{pageLink.Id}".Trim('/');
        }
    }
}
