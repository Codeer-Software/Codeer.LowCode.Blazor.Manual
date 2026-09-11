using Codeer.LowCode.Blazor.Extras.Mail;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Microsoft.AspNetCore.Mvc;
using LowCodeSamples.Server.Services;

namespace LowCodeSamples.Server.Controllers
{
    //メール送信 (MailField / BulkMailField) の受け口。ロジックは Extras.Server にあり、ここは結線だけを持つ
    [ApiController]
    [Route("api/mail")]
    public class MailController : ControllerBase, IAsyncDisposable
    {
        readonly DataService _dataService;
        readonly ILogger<MailController> _logger;

        public MailController(DataService dataService, ILogger<MailController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //デモサイトではメールを送信しない (プレビューで解決結果を確認できる)
        static void CheckCanUpdate()
        {
            if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためメールは送信できません");
        }

        //単発送信 (ModuleDataIO を渡す = リクエストの MailField が今のユーザーに見えるときだけ送る)
        [HttpPost]
        public async Task<MailSendResult> SendEmailAsync(MailSendRequest request)
        {
            CheckCanUpdate();
            return await CreateDispatcher().SendAsync(request, _dataService.ModuleDataIO);
        }

        //一斉送信 (宛先はサーバーで検索条件から解決。読み取り権限が効き、宛先一覧はクライアントに渡らない)
        [HttpPost("bulk_search")]
        public async Task<MailSendResult> SendBulkSearchAsync(MailBulkSearchRequest request)
        {
            CheckCanUpdate();
            return await new MailBulkSearch(CreateDispatcher(), _dataService.ModuleDataIO, DesignerService.GetDesignData(),
                    e => _logger.LogError("{Error}", e))
                .SendAsync(request);
        }

        //プレビュー (送らずに解決結果を HTML で返す。宛先の解決は送信と同じ経路)
        [HttpPost("preview")]
        public async Task<IActionResult> PreviewAsync(MailPreviewRequest request)
            => PreviewFile(await CreatePreviewBuilder().BuildSingleHtmlAsync(request));

        [HttpPost("bulk_preview")]
        public async Task<IActionResult> PreviewBulkSearchAsync(MailBulkSearchRequest request)
            => PreviewFile(await CreatePreviewBuilder().BuildBulkHtmlAsync(request));

        MailPreviewBuilder CreatePreviewBuilder()
            => new(CreateDispatcher(), _dataService.ModuleDataIO, DesignerService.GetDesignData());

        IActionResult PreviewFile(string html)
            => File(System.Text.Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8", "mail-preview.html");

        MailDispatcher CreateDispatcher()
        {
            var mail = SystemConfig.Instance.Mail;
            //履歴はシステムの記録なので、操作ユーザーの書き込み権限に依存しない内部経路で書く
            var historyWriter = string.IsNullOrEmpty(mail.HistoryModuleName)
                ? null
                : new MailHistoryWriter(mail.HistoryModuleName, DesignerService.GetDesignData(),
                    data => _dataService.ModuleDataIO.AddSystemRecordAsync(data), e => _logger.LogError("{Error}", e));
            return new MailDispatcher(mail, CreateSender, historyWriter);
        }

        //呼び名→送信インフラの対応表は MailSenderTable (独自インフラはそこに足す)
        static IMailSender? CreateSender(string name) => MailSenderTable.Create(name);
    }
}
