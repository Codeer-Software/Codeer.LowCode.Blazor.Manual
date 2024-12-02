using AccessSample.Client.Shared.ScriptObjects;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Transfer;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace AccessSample.Client.Shared.Services
{
    public class AppInfoService : IAppInfoService
    {
        readonly NavigationManager _navigationManager;
        readonly HttpService _http;
        readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
        readonly ToasterEx _toaster;
        HubConnection? _hubConnection;
        bool? _useHotReload;
        DesignData? _design;
        DateTime _lastHotReload = DateTime.Now;

        public ModuleData? CurrentUserData { get; private set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public Guid Guid { get; set; } = Guid.NewGuid();

        public event EventHandler OnHotReload = delegate { };

        public bool IsDesignMode => false;

        public DesignData GetDesignData() => _design ?? new();

        public AppInfoService(HttpService http, NavigationManager navigationManager, ILogger logger, ToasterEx toaster)
        {
            _http = http;
            _navigationManager = navigationManager;
            _toaster = toaster;
            _scriptRuntimeTypeManager.AddCustomInjector(() => http);
            _scriptRuntimeTypeManager.AddType(typeof(ScriptObjects.Excel));
            _scriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            _scriptRuntimeTypeManager.AddType<WebApiResult>();
            _scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
            _scriptRuntimeTypeManager.AddService(new Toaster(toaster));
            _scriptRuntimeTypeManager.AddService(new MailService());
        }

        public async Task InitializeAppAsync()
        {
            await InitializeHotReloadAsync();
            if (_design != null) return;

            _design = DesignDataTransferLogic.ToDesignData(await _http.GetFromStreamAsync($"/api/module_data/design"));
            var currentUserModule = _design.Modules.Find(_design.AppSettings.CurrentUserModuleDesignName);
            if (currentUserModule == null || string.IsNullOrEmpty(CurrentUserId)) return;
            CurrentUserData = (await _http.PostAsJsonAsync<SearchCondition, Paging<ModuleData>>($"/api/module_data/list",
                new()
                {
                    ModuleName = currentUserModule.Name,
                    Condition = new FieldValueMatchCondition { SearchTargetVariable = "Id.Value", Comparison = MatchComparison.Equal, Value = MultiTypeValue.Create(CurrentUserId) }
                }))?.Items.FirstOrDefault();
        }

        public ScriptRuntimeTypeManager GetScriptRuntimeTypeManager()
        => _scriptRuntimeTypeManager;

        public async Task<MemoryStream?> GetResourceAsync(string resourcePath)
        {
            var result = await _http.GetAsync($"/api/module_data/resource?resource={resourcePath}");
            if (result == null) return null;
            return (MemoryStream)await result.Content.ReadAsStreamAsync();
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
            if (_useHotReload == null)
            {
                _useHotReload = (await _http.GetFromJsonAsync<ValueWrapper<bool>>($"/api/module_data/use_hot_reload"))?.Value;
            }

            if (_useHotReload == true && _hubConnection == null)
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
    }
}
