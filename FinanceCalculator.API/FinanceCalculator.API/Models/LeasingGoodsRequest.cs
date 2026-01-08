namespace FinanceCalculator.API.Models
{
    public class LeasingGoodsRequest
    {
        public decimal ItemPrice { get; set; }
        public decimal DownPayment { get; set; }     
        public int TermMonths { get; set; }    
        public decimal MonthlyPayment { get; set; }
        public decimal ProcessingFeePercent { get; set; }
    }
}
