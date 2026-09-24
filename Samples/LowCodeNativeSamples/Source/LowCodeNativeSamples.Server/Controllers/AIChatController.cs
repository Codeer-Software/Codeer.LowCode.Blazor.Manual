using Codeer.LowCode.Blazor.Extras.AIChat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using LowCodeNativeSamples.Server.AI;
using LowCodeNativeSamples.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LowCodeNativeSamples.Server.Controllers
{
    //AIChatField の受け口。送信は即 requestId を返し (202)、クライアントは GET でポーリングする。
    //返事を作る Agent は AI/AIChatAgentTable (Agent 名 → Agent の対応表) で選ばれる。AIChatField のデザインの Agent 名が鍵。ロジックは Extras.Server にあり、ここは結線だけを持つ
    //送信は ModuleDataIO を渡す = リクエストの AIChatField が今のユーザーに見えるときだけ受け付ける (アプリアクセス条件・モジュールの UserRead・フィールド読取権限)
    [Authorize, AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("api/ai_chat")]
    public class AIChatController : ControllerBase, IAsyncDisposable
    {
        static AIChatService _aiChat => AIChatAgentTable.Service;

        readonly DataService _dataService;

        public AIChatController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //ジョブと会話履歴の所有者。他人のジョブは見えない。表示名は同名・改名がありうるのでユーザー ID を優先する
        string Owner => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? string.Empty;

        [HttpPost]
        public async Task<ActionResult<AIChatSendResponse>> Send([FromBody] AIChatSendRequest request)
            => Accepted(new AIChatSendResponse { RequestId = await _aiChat.StartAsync(Owner, request, _dataService.ModuleDataIO) });

        [HttpGet("{requestId}")]
        public ActionResult<AIChatStatusResponse> Status(string requestId)
        {
            var status = _aiChat.GetStatus(Owner, requestId);
            return status == null ? NotFound() : status;
        }

        [HttpDelete("{requestId}")]
        public IActionResult Cancel(string requestId)
            => _aiChat.Cancel(Owner, requestId) ? NoContent() : NotFound();
    }
}
