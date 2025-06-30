using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using WebApp.Client.Shared.ScriptObjects;
using WebApp.Client.Shared.Services;

namespace WebApp.Services
{
    public class AppInfoService : IAppInfoServiceExtension, IAsyncDisposable, IDisposable
    {
        readonly NavigationManager _navigationManager;
        readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
        readonly ToasterEx _toaster;
        DesignData? _design;
        DateTime _lastHotReload = DateTime.Now;
        SystemConfigForFront? _config;
        HubConnection? _hubConnection;
        LocalizeService? _localizeService;

        public ModuleData? CurrentUserData { get; private set; }

        public string CurrentUserId { get; private set; } = string.Empty;

        public void SetCurrentUserId(string id) => CurrentUserId = id;

        public Guid Guid { get; set; } = Guid.NewGuid();

        public event EventHandler OnHotReload = delegate { };

        public bool IsDesignMode => false;

        public DesignData GetDesignData() => _design ?? new();

        //Debugging is not possible with ServerSide Blazor
        public bool CanScriptDebug => false;

        public string Localize(string text)
            => _localizeService?.Localize(text) ?? text;

        public AppInfoService(HttpService http, Codeer.LowCode.Blazor.RequestInterfaces.ILogger logger, ToasterEx toaster, NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            _toaster = toaster;
            _scriptRuntimeTypeManager.AddCustomInjector(() => http);
            _scriptRuntimeTypeManager.AddType(typeof(WebApp.Client.Shared.ScriptObjects.Excel));
            _scriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            _scriptRuntimeTypeManager.AddType<WebApiResult>();
            _scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
            _scriptRuntimeTypeManager.AddService(new Toaster(toaster));
        }

        public async Task InitializeAppAsync()
        {
            await InitializeHotReloadAsync();
            if (_design != null) return;
            await Task.CompletedTask;
            _design = DesignerService.GetDesignDataForFront(null);
            _localizeService = await this.CreateLocalizeService();
        }

        public ScriptRuntimeTypeManager GetScriptRuntimeTypeManager()
            => _scriptRuntimeTypeManager;

        public async Task<MemoryStream?> GetResourceAsync(string resourcePath)
        {
            await Task.CompletedTask;
            return DesignerService.GetResource(resourcePath ?? string.Empty);
        }

        public void ClearDesignData()
        {
            _toaster.Clear();
            Guid = Guid.NewGuid();
            _design = null;
            CurrentUserData = null;
        }

        async Task InitializeHotReloadAsync()
        {
            lock (this)
            {
                if (_config == null)
                {
                    _config = SystemConfig.Instance.ForFront();
                }
                else
                {
                    return;
                }
            }
            if (_config?.UseHotReload == true && _hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri("/hot_reload_hub"))
                    .Build();

                _hubConnection.On("ExecuteHotReload", async () =>
                {
                    //Adjustments as there are times when a single request comes multiple times.
                    var now = DateTime.Now;
                    if (now - _lastHotReload < TimeSpan.FromSeconds(3)) return;

                    _lastHotReload = now;

                    ClearDesignData();
                    await InitializeAppAsync();
                    OnHotReload?.Invoke(this, EventArgs.Empty);
                });
                await _hubConnection.StartAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection == null) return;
            await _hubConnection.DisposeAsync();
            await Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
