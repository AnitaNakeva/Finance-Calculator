using System;

namespace FinanceCalculator.API.Models
{
    public class FavoriteCalculation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CalculationType { get; set; } = string.Empty;
        public string RequestJson { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
