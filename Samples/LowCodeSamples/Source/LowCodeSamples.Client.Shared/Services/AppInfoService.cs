using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Transfer;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using LowCodeSamples.Client.Shared.ScriptObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace LowCodeSamples.Client.Shared.Services
{
    public class AppInfoService : IAppInfoService
    {
        readonly NavigationManager _navigationManager;
        readonly HttpService _http;
        readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
        readonly ToasterEx _toaster;
        readonly LoadingService _loadingService;
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

        public AppInfoService(HttpService http, LoadingService loadingService, NavigationManager navigationManager, ILogger logger, ToasterEx toaster)
        {
            _http = http;
            _navigationManager = navigationManager;
            _toaster = toaster;
            _loadingService = loadingService;
            _scriptRuntimeTypeManager.AddCustomInjector(() => http);
            _scriptRuntimeTypeManager.AddType(typeof(ScriptObjects.Excel));
            _scriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            _scriptRuntimeTypeManager.AddType<WebApiResult>();
            _scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
            _scriptRuntimeTypeManager.AddService(new Toaster(toaster));
        }

        public async Task InitializeAppAsync()
        {
            using var scope = _loadingService.StartLoading(200);

            await InitializeHotReloadAsync();
            if (_design != null) return;

            _design = DesignDataTransferLogic.ToDesignData(await _http.GetFromStreamAsync($"/api/module_data/design"));
            var currentUserModule = _design.Modules.Find(_design.AppSettings.CurrentUserModuleDesignName);
            if (currentUserModule == null || string.IsNullOrEmpty(CurrentUserId)) return;
            var currentUserRequest = new GetListRequest
            {
                Condition = new()
                {
                    ModuleName = currentUserModule.Name,
                    Condition = new FieldValueMatchCondition { SearchTargetVariable = "Id.Value", Comparison = MatchComparison.Equal, Value = MultiTypeValue.Create(CurrentUserId) }
                }
            };
            CurrentUserData = (await ModuleDataService.GetListAsync(_http, [currentUserRequest]))?.FirstOrDefault()?.Items.FirstOrDefault();
        }

        public ScriptRuntimeTypeManager GetScriptRuntimeTypeManager()
            => _scriptRuntimeTypeManager;

        public async Task<MemoryStream?> GetResourceAsync(string resourcePath)
        {
            var result = await _http.GetAsync($"/api/module_data/resource?resource={resourcePath}", false);
            if (result == null) return null;
            return (MemoryStream)await result.Content.ReadAsStreamAsync();
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
                    .WithUrl(_http.BaseUrl + "hot_reload_hub")
                    .Build();

                _hubConnection.On("ExecuteHotReload", async () =>
                {
                    //Adjustments as there are times when a single request comes multiple times.
                    var now = DateTime.Now;
                    if (now - _lastHotReload < TimeSpan.FromSeconds(3)) return;

                    _lastHotReload = now;

                    ClearByHotReload();
                    await InitializeAppAsync();
                    OnHotReload?.Invoke(this, EventArgs.Empty);
                });
                await _hubConnection.StartAsync();
            }
        }

        void ClearByHotReload()
        {
            _toaster.Clear();
            Guid = Guid.NewGuid();
            _design = null;
            CurrentUserData = null;
        }
    }
}
