using Codeer.LowCode.Blazor.Extras.Server.Mail;

namespace LowCodeSamples.Server.Services
{
    /// <summary>
    /// 「送信先の呼び名」→ 送信インフラ実装 (<see cref="IMailSender"/>) の対応表。
    /// 呼び名はフィールドの MailInfraName / appsettings の Mail.DefaultInfraName で指定する。
    /// プロバイダごとの設定は appsettings の独立したセクション ("Smtp" / "GraphApi" / "Gmail") で、Program.cs が個別に読む。
    /// 独自インフラを使うときは <see cref="IMailSender"/> を実装してこの switch に1行足す。
    /// null を返すと「その呼び名は対応表に無い」エラーになる (黙って別のインフラで送らない)。
    /// </summary>
    public static class MailSenderTable
    {
        public static IMailSender? Create(string name)
        {
            var config = SystemConfig.Instance;
            return name switch
            {
                "Smtp" => new SmtpMailSender(config.Smtp),
                "GraphApi" => new GraphApiMailSender(config.GraphApi),
                "Gmail" => new GmailApiMailSender(config.Gmail),
                _ => null,
            };
        }
    }
}
