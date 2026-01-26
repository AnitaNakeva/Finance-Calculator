using System;

namespace FinanceCalculator.API.Models
{
    public class CalculationHistoryView
    {
        public int Id { get; set; }
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
        public decimal? InitialFees { get; set; }
        public decimal? MonthlyFees { get; set; }
        public decimal? AnnualFees { get; set; }
        public decimal? TotalFees { get; set; }
        public decimal? AnnualPercentageRate { get; set; }
    }
}
