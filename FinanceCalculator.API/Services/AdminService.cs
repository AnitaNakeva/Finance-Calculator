using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalculator.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;

        public AdminService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<object>> GetUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Id)
                .Select(u => new { u.Id, u.Username, u.Role })
                .ToListAsync();
        }

        public async Task<(bool success, string? error)> ChangeRoleAsync(int userId, string role)
        {
            if (role != "Admin" && role != "User")
            {
                return (false, "Role must be Admin or User.");
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, null);

            user.Role = role;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<object>> GetCalculationsAsync(int? userId)
        {
            var query = _db.CalculationRecords
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAtUtc)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            return await query
                .Take(100)
                .Select(r => new
                {
                    r.Id,
                    r.CalculationType,
                    r.UserId,
                    Username = r.User != null ? r.User.Username : string.Empty,
                    r.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAuditAsync(int? userId)
        {
            var query = _db.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.TimestampUtc)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            return await query
                .Take(200)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    Username = a.User != null ? a.User.Username : string.Empty,
                    a.Event,
                    a.TimestampUtc
                })
                .ToListAsync();
        }

    }
}
