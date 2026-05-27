using Codeer.LowCode.Blazor.DataIO;

namespace LowCodeApp.WPF.LowCodeApp.Services
{
    public class AuthenticationContext : IAuthenticationContext
    {
        public async Task<string> GetCurrentUserIdAsync()
        {
            await Task.CompletedTask;
            return string.Empty;
        }
    }

}
