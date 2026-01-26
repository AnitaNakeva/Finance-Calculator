
namespace FinanceCalculator.API.Models
{
    public class LeasingGoodsResponce
    {
        public decimal FinancedAmount { get; set; }     
        public decimal ProcessingFeeAmount { get; set; } 
        public decimal TotalPaid { get; set; }            
        public decimal OverpaymentAmount { get; set; }    
        public decimal OverpaymentPercent { get; set; }   

      
        public List<ScheduleItem> Schedule { get; set; } = new();
    }
}
