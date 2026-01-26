using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface IAdminService
    {
        Task<IEnumerable<object>> GetUsersAsync();
        Task<(bool success, string? error)> ChangeRoleAsync(int userId, string role);
        Task<IEnumerable<object>> GetCalculationsAsync(int? userId);
        Task<IEnumerable<object>> GetAuditAsync(int? userId);
    }
}
