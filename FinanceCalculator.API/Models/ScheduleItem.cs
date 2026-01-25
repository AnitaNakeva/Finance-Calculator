namespace FinanceCalculator.API.Models
{
    public class ScheduleItem
    {
        public int Month { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal Payment { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
