using Codeer.LowCode.Blazor.RequestInterfaces;

namespace WebApp.Client.Shared.Services
{
    public interface IAppInfoServiceExtension : IAppInfoService
    {
        Guid Guid { get; set; }
        event EventHandler OnHotReload;
        Task InitializeAppAsync();
        void SetCurrentUserId(string id);
    }
}
