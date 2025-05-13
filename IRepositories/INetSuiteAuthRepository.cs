using AuthService.Models;

namespace AuthService.IRepositories
{
    public interface INetSuiteAuthRepository
    {
        Task<TokenResponse?> ExchangeCodeForTokenAsync(string code);

        string BuildAuthorizationUrl(string? customState = null);
    }
}