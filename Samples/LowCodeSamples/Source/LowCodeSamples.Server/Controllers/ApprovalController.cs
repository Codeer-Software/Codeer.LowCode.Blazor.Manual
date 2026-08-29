using Codeer.LowCode.Blazor.Extras.Approval;
using Codeer.LowCode.Blazor.Extras.Server.Approval;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
{
    //承認フローの command API。状態遷移の唯一の口 (クライアントは承認モジュールを直接書けない)。
    //ロジックは ApprovalEngine (Extras.Server) にあり、Controller は結線だけを持つ
    [ApiController]
    [Route("api/approval")]
    public class ApprovalController : ControllerBase, IAsyncDisposable
    {
        readonly DataService _dataService;
        readonly ILogger<ApprovalController> _logger;

        public ApprovalController(DataService dataService, ILogger<ApprovalController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //すべての操作 (申請/再申請/承認/却下/差し戻し/取り下げ/確認) を 1 本で受け、ApprovalEngine が Action で振り分ける
        //(デモサイトではデータ更新と同じく実行しない。状態表示・ボタンの出し分けは操作ユーザー = DemoUserId の視点で確認できる)
        [HttpPost]
        public async Task<ApprovalActionResult> ExecuteAsync(ApprovalCommand command)
        {
            if (!SystemConfig.Instance.CanUpdate) throw new Exception("デモ用のためデータの更新はできません");
            return await CreateEngine().ExecuteAsync(command);
        }

        //承認データの書き込みはシステムの記録なので、操作ユーザーの書き込み権限に依存しない内部経路で行う
        ApprovalEngine CreateEngine()
        {
            var mail = SystemConfig.Instance.Mail;
            //送信履歴はシステムの記録なので内部経路で書く (MailController と同じ)
            var historyWriter = string.IsNullOrEmpty(mail.HistoryModuleName)
                ? null
                : new MailHistoryWriter(mail.HistoryModuleName, DesignerService.GetDesignData(),
                    data => _dataService.ModuleDataIO.AddSystemRecordAsync(data), e => _logger.LogError("{Error}", e));
            return new(DesignerService.GetDesignData(), _dataService.ModuleDataIO, _dataService.DbAccess,
                data => _dataService.ModuleDataIO.AddSystemRecordAsync(data),
                data => _dataService.ModuleDataIO.UpdateSystemRecordAsync(data))
            {
                //順番到達の通知メール (メンバー契約の TurnNotifyMail が設定されているときだけ送られる)
                MailDispatcher = new MailDispatcher(mail, name => MailSenderTable.Create(name), historyWriter: historyWriter),
                LogError = e => _logger.LogError("{Error}", e),
            };
        }
    }
}
