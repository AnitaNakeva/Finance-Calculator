using System;

namespace FinanceCalculator.API.Models
{
    public class CalculationRecord
    {
        public int Id { get; set; }
        public string CalculationType { get; set; } = string.Empty;
        public string RequestJson { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
