namespace FinanceCalculator.API.Models
{
    public class CreditRequest
    {
        public decimal Principal { get; set; }        // сума
        public int TermMonths { get; set; }           // срок
        public decimal AnnualInterestRate { get; set; } // 0.05 = 5%
        public PaymentType PaymentType { get; set; }

        // промо период (по избор)

        public int GraceMonths { get; set; }
        public int PromoMonths { get; set; }
        public decimal PromoAnnualInterestRate { get; set; }
    }

}
