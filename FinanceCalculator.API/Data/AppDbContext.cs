using FinanceCalculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalculator.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // history table
        public DbSet<CalculationRecord> CalculationRecords { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RevokedToken> RevokedTokens { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        // favourites table
        public DbSet<FavoriteCalculation> FavoriteCalculations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<CalculationRecord>()
                .HasOne(r => r.User)
                .WithMany(u => u.CalculationRecords)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RevokedToken>()
                .HasIndex(r => r.Jti)
                .IsUnique();

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteCalculation>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteCalculation>()
                .HasIndex(f => new { f.UserId, f.Name })
                .IsUnique();
        }
    }
}
