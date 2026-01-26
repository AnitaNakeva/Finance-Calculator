namespace FinanceCalculator.API.Models
{
    public class CreditRequest
    {
        public decimal Principal { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public PaymentType PaymentType { get; set; }

        public int GraceMonths { get; set; }
        public int PromoMonths { get; set; }
        public decimal PromoAnnualInterestRate { get; set; }

        public decimal ApplicationFee { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal OtherInitialFees { get; set; }

        public decimal MonthlyManagementFee { get; set; }
        public decimal OtherMonthlyFees { get; set; }

        public decimal AnnualManagementFee { get; set; }
        public decimal OtherAnnualFees { get; set; }
    }

}
