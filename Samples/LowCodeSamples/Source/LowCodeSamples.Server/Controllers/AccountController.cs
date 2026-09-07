using Codeer.LowCode.Blazor.Utils;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
{
    //デモサイトは認証を持たない (ログイン画面なし)。Cookie 認証テンプレートと同じ URL で固定の操作ユーザーを返し、
    //クライアント (Web / MAUI) はテンプレートと同じ流れで現在ユーザーを解決する
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        [HttpGet("current_user")]
        public StringWrapper GetCurrentUser()
            => new(DataService.DemoUserId);
    }
}
