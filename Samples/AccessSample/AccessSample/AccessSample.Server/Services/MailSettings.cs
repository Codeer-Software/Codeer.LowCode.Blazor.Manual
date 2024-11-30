namespace AccessSample.Server.Services
{
    public class MailSettings
    {
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string SenderMailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SSL { get; set; } = string.Empty;
    }
}
