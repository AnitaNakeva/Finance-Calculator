namespace FinanceCalculator.API.Models
{
    using System.Collections.Generic;

    public class CreditResponse
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPaid { get; set; }

        public List<ScheduleItem> Schedule { get; set; } = new List<ScheduleItem>();
    }

}
