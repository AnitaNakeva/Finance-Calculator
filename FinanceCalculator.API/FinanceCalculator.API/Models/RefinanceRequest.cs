namespace FinanceCalculator.API.Models
{
    public class RefinanceRequest
    {
        // ====== Current loan (existing) ======
        public decimal CurrentPrincipal { get; set; }           // Размер на кредита
        public int CurrentTermMonths { get; set; }              // Срок (месеци)
        public decimal CurrentAnnualInterestRate { get; set; }  // Лихва (%)
        public int PaymentsMade { get; set; }                   // Брой направени вноски
        public decimal EarlyRepaymentFeePercent { get; set; }   // Такса за предсрочно погасяване (%)

        // ====== New loan (refinance) ======
        public decimal NewAnnualInterestRate { get; set; }      // Лихва (%)
        public decimal UpfrontFeesPercent { get; set; }         // Първоначални такси (%)
        public decimal UpfrontFeesFixed { get; set; }           // Първоначални такси (валута)
    }
}
