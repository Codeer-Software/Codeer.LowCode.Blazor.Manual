namespace LowCodeApp.Cookie.Server
{
    public class PasswordCheckUser
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
