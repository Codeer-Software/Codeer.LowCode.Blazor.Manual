using Codeer.LowCode.Blazor.Extras.Api;
using Codeer.LowCode.Blazor.Extras.Server.Api;
using Codeer.LowCode.Blazor.Extras.Server.Jobs;
using LowCodeSamples.Server.Jobs;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
{
    //Extras の共通の受け口 (POST /api/extras)。デモサイトは他の機能を専用の Controller で受けているので、ここに並べるのはジョブ (JobTriggerField) だけ
    [ApiController]
    [Route("api/extras")]
    public class ExtrasController : ControllerBase, IAsyncDisposable
    {
        static readonly ExtrasApi _api = new()
        {
            Handlers =
            {
                new JobTriggerApi(() => JobRuntimeHost.Names, JobRuntimeHost.RunFromScreenAsync),
            },
        };

        readonly DataService _dataService;
        readonly ILogger<ExtrasController> _logger;

        public ExtrasController(DataService dataService, ILogger<ExtrasController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        [HttpPost]
        public async Task<ActionResult<ExtrasResponse>> ExecuteAsync(ExtrasRequest request)
        {
            var response = await _api.ExecuteAsync(request, new ExtrasApiContext
            {
                ModuleDataIO = _dataService.ModuleDataIO,
                DesignData = DesignerService.GetDesignData(),
                DbAccess = _dataService.DbAccess,
                UserId = await _dataService.GetCurrentUserIdAsync(),
                Logger = _logger,
            });
            return response == null ? NotFound() : response;
        }
    }
}
