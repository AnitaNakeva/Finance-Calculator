using System;

namespace FinanceCalculator.API.Models
{
    public class RevokedToken
    {
        public int Id { get; set; }
        public string Jti { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
