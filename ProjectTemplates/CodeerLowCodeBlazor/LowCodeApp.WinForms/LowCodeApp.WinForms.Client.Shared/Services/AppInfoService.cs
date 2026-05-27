using Codeer.LowCode.Bindings.ApexCharts;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Transfer;
using Codeer.LowCode.Blazor.Extras;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using LowCodeApp.WinForms.Client.Shared.ScriptObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace LowCodeApp.WinForms.Client.Shared.Services
{
    public interface IAppInfoServiceExtension : IAppInfoService
    {
        Task InitializeAppAsync();
        void SetCurrentUserId(string id);
    }

    public class AppInfoService : IAppInfoServiceExtension
    {
        readonly NavigationManager _navigationManager;
        readonly HttpService _http;
        readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
        readonly LoadingService _loadingService;
        HubConnection? _hubConnection;
        DesignData? _design;
        SystemConfigForFront? _config;
        LocalizeService? _localizeService;

        public ModuleData? CurrentUserData { get; private set; }

        public string CurrentUserId { get; private set; } = string.Empty;

        public DesignData GetDesignData() => _design ?? new();

        public bool CanScriptDebug => _config?.CanScriptDebug == true;

        public string Localize(string text)
            => _localizeService?.Localize(text) ?? text;

        public AppInfoService(HttpService http, LoadingService loadingService, NavigationManager navigationManager, ILogger logger, ToasterEx toaster)
        {
            _http = http;
            _navigationManager = navigationManager;
            _loadingService = loadingService;
            _scriptRuntimeTypeManager.AddCustomInjector(() => http);
            _scriptRuntimeTypeManager.AddType(typeof(ScriptObjects.Excel));
            _scriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
            _scriptRuntimeTypeManager.AddType<WebApiResult>();
            _scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
            _scriptRuntimeTypeManager.AddService(new Toaster(toaster));
            _scriptRuntimeTypeManager.AddService(new MailService());
            _scriptRuntimeTypeManager.AddService(loadingService);
            _scriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();
            ApexChartsClientInitializer.Initialize(this);
            ExtrasClientInitializer.Initialize(this);
        }
        public void SetCurrentUserId(string id) => CurrentUserId = id;

        public async Task InitializeAppAsync()
        {
            using var scope = _loadingService.StartLoading(int.MaxValue);

            await InitializeHotReloadAsync();
            if (_design != null) return;

            using var designDataStream = await _http.GetFromStreamAsync($"/api/module_data/design");
            _design = DesignDataTransferLogic.ToDesignData(designDataStream);
            _localizeService = await this.CreateLocalizeService();

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
            _config ??= await _http.GetFromJsonAsync<SystemConfigForFront>($"/api/module_data/config");
            if (_config?.UseHotReload != true || _hubConnection != null) return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/hot_reload_hub"))
                .Build();

            _hubConnection.On("ExecuteHotReload", () => _navigationManager.Refresh(true));
            await _hubConnection.StartAsync();
        }
    }
}
