using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);

        Task LogoutAsync(int userId, string jti);
    }
}
