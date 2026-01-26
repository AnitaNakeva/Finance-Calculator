using System;

namespace FinanceCalculator.API.Models
{
    public class ParsedCalculation
    {
        public string CalculationType { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public decimal? Principal { get; set; }
        public int? TermMonths { get; set; }
        public decimal? AnnualRate { get; set; }
        public string? PaymentType { get; set; }
        public string? MonthlyPayment { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal? TotalInterest { get; set; }
        public decimal? FinancedAmount { get; set; }
        public decimal? OverpaymentPercent { get; set; }
        public decimal? Savings { get; set; }
        public decimal? CurrentCloseCost { get; set; }
    }
}
