
using System.Collections.Generic;

namespace FinanceCalculator.API.Models
{
    public class RefinaceResponce
    {

        public int RemainingMonths { get; set; }


        public decimal CurrentMonthlyPayment { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public decimal CurrentTotalPaidRemaining { get; set; }
        public decimal EarlyRepaymentFeeAmount { get; set; }
        public decimal CurrentTotalCostToClose { get; set; }


        public decimal NewLoanPrincipal { get; set; }
        public decimal NewMonthlyPayment { get; set; }
        public decimal UpfrontFeesPercentAmount { get; set; }
        public decimal UpfrontFeesFixedAmount { get; set; }
        public decimal NewTotalPaid { get; set; }


        public decimal Savings { get; set; }


        public List<ScheduleItem> CurrentRemainingSchedule { get; set; } = new();
        public List<ScheduleItem> NewLoanSchedule { get; set; } = new();
    }
}

