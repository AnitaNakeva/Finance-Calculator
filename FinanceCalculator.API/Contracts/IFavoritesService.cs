using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface IFavoritesService
    {
        Task<IEnumerable<FavoriteCalculation>> ListAsync(int userId);
        Task<FavoriteCalculation?> GetAsync(int userId, int id);
        Task<bool> DeleteAsync(int userId, int id);
        Task<(bool success, string? conflict)> RenameAsync(int userId, int id, string newName);
    }
}
