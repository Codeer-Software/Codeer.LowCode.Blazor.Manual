using Codeer.LowCode.Blazor.Extras.AIChat;
using Codeer.LowCode.Blazor.Extras.Server.AI.Chat;
using LowCodeNativeSamples.Server.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LowCodeNativeSamples.Server.Controllers
{
    //AIChatField の受け口。送信は即 requestId を返し (202)、クライアントは GET でポーリングする。
    //返事を作る Agent は AI/AIChatAgentTable (Agent 名 → Agent の対応表) で選ばれる。AIChatField のデザインの Agent 名が鍵。ロジックは Extras.Server にあり、ここは結線だけを持つ
    [Authorize, AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("api/ai_chat")]
    public class AIChatController : ControllerBase
    {
        static AIChatJobStore _jobs => AIChatAgentTable.Jobs;

        //ジョブと会話履歴の所有者。他人のジョブは見えない。表示名は同名・改名がありうるのでユーザー ID を優先する
        string Owner => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? string.Empty;

        [HttpPost]
        public ActionResult<AIChatSendResponse> Send([FromBody] AIChatSendRequest request)
            => Accepted(new AIChatSendResponse { RequestId = _jobs.Start(Owner, request.ConversationId, request.Message, request.Agent, request.DocumentFolder, request.Transcript) });

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
