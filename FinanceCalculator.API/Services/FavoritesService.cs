using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Data;
using FinanceCalculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalculator.API.Services
{
    public class FavoritesService : IFavoritesService
    {
        private readonly AppDbContext _db;

        public FavoritesService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FavoriteCalculation>> ListAsync(int userId)
        {
            return await _db.FavoriteCalculations
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<FavoriteCalculation?> GetAsync(int userId, int id)
        {
            return await _db.FavoriteCalculations.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var favorite = await _db.FavoriteCalculations.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (favorite == null) return false;
            _db.FavoriteCalculations.Remove(favorite);
            await _db.SaveChangesAsync();
            return true;
        }

        // using conflict to get the reason if failing
        public async Task<(bool success, string? conflict)> RenameAsync(int userId, int id, string newName)
        {
            var favorite = await _db.FavoriteCalculations.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (favorite == null) return (false, null);

            var exists = await _db.FavoriteCalculations.AnyAsync(f => f.UserId == userId && f.Name == newName && f.Id != id);
            if (exists)
            {
                return (false, "Favorite with that name already exists.");
            }

            favorite.Name = newName;
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
