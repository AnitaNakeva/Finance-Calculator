using System;
using System.Collections.Generic;

namespace FinanceCalculator.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public string Role { get; set; } = "User";

        public ICollection<CalculationRecord> CalculationRecords { get; set; } = new List<CalculationRecord>();
    }
}
