using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using WebApp.Client.Shared.Services;

namespace WebApp.Client.Shared.ScriptObjects
{
    public class MailRequest
    {
        public string Address { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class MailService
    {
        [ScriptInject]
        public HttpService? Http { get; set; }

        [ScriptHide]
        public static Func<string, string, string, Task<bool>>? SendEmailAsyncCore { get; set; }

        [ScriptName("SendEmail")]
        public async Task<bool> SendEmailAsync(string address, string subject, string message)
        {
            if (SendEmailAsyncCore != null) return await SendEmailAsyncCore(address, subject, message);
            if (Http == null) return false;
            var ret = await Http.PostAsJsonAsync<MailRequest, ValueWrapper<bool>>("/api/mail", new MailRequest { Address = address, Subject = subject, Message = message });
            return ret?.Value ?? false;
        }
    }
}
