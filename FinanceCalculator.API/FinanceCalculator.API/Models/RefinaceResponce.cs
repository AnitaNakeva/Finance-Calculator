
using System.Collections.Generic;

namespace FinanceCalculator.API.Models
{
    public class RefinaceResponce
    {
        // Remaining term for refinance (computed)
        public int RemainingMonths { get; set; }

        // Current loan results
        public decimal CurrentMonthlyPayment { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public decimal CurrentTotalPaidRemaining { get; set; } // remaining installments sum
        public decimal EarlyRepaymentFeeAmount { get; set; }   // % over remaining principal
        public decimal CurrentTotalCostToClose { get; set; }   // remaining installments + early fee

        // New loan results
        public decimal NewLoanPrincipal { get; set; }          // remaining principal + upfront fees
        public decimal NewMonthlyPayment { get; set; }
        public decimal UpfrontFeesPercentAmount { get; set; }
        public decimal UpfrontFeesFixedAmount { get; set; }
        public decimal NewTotalPaid { get; set; }              // installments sum (principal already includes fees)

        // Comparison
        public decimal Savings { get; set; }                   // CurrentTotalCostToClose - NewTotalPaid

        // Schedules (same ScheduleItem type as your first calculator)
        public List<ScheduleItem> CurrentRemainingSchedule { get; set; } = new();
        public List<ScheduleItem> NewLoanSchedule { get; set; } = new();
    }
}
