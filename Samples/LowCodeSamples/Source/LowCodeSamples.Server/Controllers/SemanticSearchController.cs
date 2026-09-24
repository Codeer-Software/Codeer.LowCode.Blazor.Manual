using Codeer.LowCode.Blazor.Extras.SemanticSearch;
using Codeer.LowCode.Blazor.Extras.Server.AI.SemanticSearch;
using LowCodeSamples.Server.AI;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LowCodeSamples.Server.Controllers
{
    //SemanticSearchField (意味検索) の再索引 API。フィールドのスクリプト Reindex / ReindexMissing の受け口。
    //開始は即 requestId を返し (202)、クライアントは GET で進捗をポーリングする (AIChatController と同じ形)。ロジックは Extras.Server にあり、ここは結線だけを持つ
    //開始はリクエストの ModuleDataIO で入口を検査する = その SemanticSearchField を今のユーザーが読めるときだけ受け付ける (アプリアクセス条件・モジュールの UserRead・フィールド読取権限)。
    //行の読み書きはバックグラウンドで、今のユーザーの権限を持つ別の DataService で行う (読める行だけ・書ける行だけ)
    [ApiController]
    [Route("api/semantic_search/reindex")]
    public class SemanticSearchController : ControllerBase, IAsyncDisposable
    {
        //意味検索のサーバー側入口はアプリの静的な持ち物 (AI/SemanticSearchIndex.cs)
        static SemanticSearchService _semanticSearch => SemanticSearchIndex.Service;

        readonly DataService _dataService;

        public SemanticSearchController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //ジョブの所有者。他人のジョブは見えない (同じモジュールの走行中ジョブに合流した人は見える)
        string Owner => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? string.Empty;

        [HttpPost]
        public async Task<ActionResult<SemanticSearchReindexResponse>> Start([FromBody] SemanticSearchReindexRequest request)
        {
            //バックグラウンド用に別の DataService を開いて渡す (デモサイトは認証が無く操作ユーザーは固定なので引数なし)
            var requestId = await _semanticSearch.StartReindexAsync(Owner, request, _dataService.ModuleDataIO, () =>
            {
                var dataService = new DataService();
                return new SemanticSearchReindexScope(dataService.ModuleDataIO, dataService.DbAccess, dataService);
            });
            return Accepted(new SemanticSearchReindexResponse { RequestId = requestId });
        }

        [HttpGet("{requestId}")]
        public ActionResult<SemanticSearchReindexStatusResponse> Status(string requestId)
        {
            var status = _semanticSearch.GetReindexStatus(Owner, requestId);
            return status == null ? NotFound() : status;
        }

        [HttpDelete("{requestId}")]
        public IActionResult Cancel(string requestId)
            => _semanticSearch.CancelReindex(Owner, requestId) ? NoContent() : NotFound();
    }
}
