namespace FinanceCalculator.API.Models
{
    public class RefinanceRequest
    {
      
        public decimal CurrentPrincipal { get; set; }           
        public int CurrentTermMonths { get; set; }              
        public decimal CurrentAnnualInterestRate { get; set; }  
        public int PaymentsMade { get; set; }                   
        public decimal EarlyRepaymentFeePercent { get; set; }   

        
        public decimal NewAnnualInterestRate { get; set; }      
        public decimal UpfrontFeesPercent { get; set; }        
        public decimal UpfrontFeesFixed { get; set; }        
    }
}

