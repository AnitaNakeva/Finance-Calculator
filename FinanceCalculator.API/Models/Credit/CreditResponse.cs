namespace FinanceCalculator.API.Models
{
    using System.Collections.Generic;

    public class CreditResponse
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal InitialFeesTotal { get; set; }
        public decimal MonthlyFeesTotal { get; set; }
        public decimal AnnualFeesTotal { get; set; }
        public decimal TotalFees { get; set; }
        public decimal AnnualPercentageRate { get; set; }
        public decimal TotalPaid { get; set; }

        public List<ScheduleItem> Schedule { get; set; } = new List<ScheduleItem>();
    }

}
