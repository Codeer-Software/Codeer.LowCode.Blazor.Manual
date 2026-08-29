using Blazor.KHandyInterop;
using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Transfer;
using Codeer.LowCode.Blazor.Extras;
using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace LowCodeSamples.Client.Shared.Services
{
    public class AppInfoService : IAppInfoService
    {
        readonly NavigationManager _navigationManager;
        readonly IHttpService _http;
        readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
        readonly IToastService _toaster;
        readonly LoadingService _loadingService;
        HubConnection? _hubConnection;
        DesignData? _design;
        DateTime _lastHotReload = DateTime.Now;
        SystemConfigForFront? _config;

        public ModuleData? CurrentUserData { get; private set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public Guid Guid { get; set; } = Guid.NewGuid();

        public event EventHandler OnHotReload = delegate { };

        public bool IsDesignMode => false;

        public DesignData GetDesignData() => _design ?? new();

        public bool CanScriptDebug => _config?.CanScriptDebug == true;

        public AppInfoService(IHttpService http, LoadingService loadingService, NavigationManager navigationManager, ILogger logger, IToastService toaster, IJSRuntime js)
        {
            _http = http;
            _navigationManager = navigationManager;
            _toaster = toaster;
            _loadingService = loadingService;
            _scriptRuntimeTypeManager.AddService(loadingService);
            _scriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();
            _scriptRuntimeTypeManager.AddService(new KJS(js));

            //Extras の組み込みスクリプトオブジェクト (Excel / WebApi / Toaster / Mail) を一括登録
            ExtrasClientInitializer.Initialize(this, http, logger, toaster);
        }

        public async Task InitializeAppAsync()
        {
            using var scope = _loadingService.StartLoading(int.MaxValue);

            if (_design != null) return;

            //設定を先に取得し、デモサイトの固定操作ユーザー (サーバーが決める) を現在ユーザーにする。
            //ホットリロード接続はデザインデータと独立なので並列に走らせる
            _config ??= await _http.GetFromJsonAsync<SystemConfigForFront>($"/api/module_data/config");
            CurrentUserId = _config?.CurrentUserId ?? string.Empty;
            var hotReloadTask = InitializeHotReloadAsync();

            using var designDataStream = await _http.GetFromStreamAsync($"/api/module_data/design");
            _design = DesignDataTransferLogic.ToDesignData(designDataStream);

            var currentUserModule = _design.Modules.Find(_design.AppSettings.CurrentUserModuleDesignName);
            if (currentUserModule != null && !string.IsNullOrEmpty(CurrentUserId))
            {
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

            await hotReloadTask;
        }

        public ScriptRuntimeTypeManager GetScriptRuntimeTypeManager()
            => _scriptRuntimeTypeManager;

        public async Task<MemoryStream?> GetResourceAsync(string resourcePath)
        {
            var result = await _http.GetAsync($"/api/module_data/resource?resource={resourcePath}", false);
            if (result == null) return null;
            return (MemoryStream)await result.Content.ReadAsStreamAsync();
        }

        public void ClearDesignData()
        {
            _toaster.Clear();
            Guid = Guid.NewGuid();
            _design = null;
            CurrentUserData = null;
            _scriptRuntimeTypeManager.ClearDesignCache();
        }

        async Task InitializeHotReloadAsync()
        {
            if (_config == null)
            {
                _config = await _http.GetFromJsonAsync<SystemConfigForFront>($"/api/module_data/config");
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
    }
}
