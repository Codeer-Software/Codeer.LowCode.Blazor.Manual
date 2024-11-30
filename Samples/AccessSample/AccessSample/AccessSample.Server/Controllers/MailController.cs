using AccessSample.Server.Services;
using Codeer.LowCode.Blazor.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace AccessSample.Server.Controllers
{
    [ApiController]
    [Route("api/mail")]
    public class MailController : ControllerBase
    {
        [HttpPost]
        public async Task<ValueWrapper<bool>> SendEmailAsync(AccessSample.Client.Shared.ScriptObjects.MailRequest request)
            => new(await SendEmailAsync(request.Address, request.Subject, request.Message));

        static async Task<bool> SendEmailAsync(string address, string subject, string message)
        {
            var config = SystemConfig.Instance.MailSettings;

            if (string.IsNullOrEmpty(config.Host)) return false;
            if (!int.TryParse(config.Port, out var port)) return false;

            using (var mailer = new SmtpClient(config.Host, port))
            {
                mailer.Credentials = new System.Net.NetworkCredential(config.SenderMailAddress, config.Password);

                if (bool.TryParse(config.SSL, out var ssl) && ssl)
                {
                    mailer.EnableSsl = true;
                }

                using (var msg = new MailMessage())
                {
                    msg.Sender = new MailAddress(config.SenderMailAddress);
                    msg.From = new MailAddress(config.SenderMailAddress);
                    foreach (var e in address.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        msg.To.Add(new MailAddress(e));
                    }
                    msg.Subject = subject;
                    msg.Body = message;
                    await mailer.SendMailAsync(msg);
                }
            }
            return true;
        }
    }
}
