using Codeer.LowCode.Blazor.Extras.AIChat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using LowCodeSamples.Server.AI;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LowCodeSamples.Server.Controllers
{
    //AIChatField の受け口。送信は即 requestId を返し (202)、クライアントは GET でポーリングする。
    //返事を作る Agent は AI/AIChatAgentTable (Agent 名 → Agent の対応表) で選ばれる。ロジックは Extras.Server にあり、ここは結線だけを持つ
    //送信は ModuleDataIO を渡す = リクエストの AIChatField が今のユーザーに見えるときだけ受け付ける (アプリアクセス条件・モジュールの UserRead・フィールド読取権限)
    [ApiController]
    [Route("api/ai_chat")]
    public class AIChatController : ControllerBase, IAsyncDisposable
    {
        static AIChatJobStore _jobs => AIChatAgentTable.Jobs;

        readonly DataService _dataService;

        public AIChatController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //デモサイト用: AI チャットの送信は 1 日 500 回まで (AITextAnalyzeController と同じ簡易な数え方)
        static int _count = 0;
        static DateTime _lastTime = DateTime.Now;
        static void Check()
        {
            _count++;
            if (24 < (DateTime.Now - _lastTime).TotalHours)
            {
                _lastTime = DateTime.Now;
                _count = 0;
            }
            if (500 < _count) throw new Exception("デモサイトの AI チャットは 1 日 500 回までです。");
        }

        //ジョブと会話履歴の所有者。デモサイトは認証が無いので空 (会話 ID だけで区別する)
        string Owner => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? string.Empty;

        [HttpPost]
        public async Task<ActionResult<AIChatSendResponse>> Send([FromBody] AIChatSendRequest request)
        {
            Check();
            return Accepted(new AIChatSendResponse { RequestId = await _jobs.StartAsync(Owner, request, _dataService.ModuleDataIO) });
        }

        [HttpGet("{requestId}")]
        public ActionResult<AIChatStatusResponse> Status(string requestId)
        {
            var status = _jobs.GetStatus(Owner, requestId);
            return status == null ? NotFound() : status;
        }

        [HttpDelete("{requestId}")]
        public IActionResult Cancel(string requestId)
            => _jobs.Cancel(Owner, requestId) ? NoContent() : NotFound();
    }
}
