using System;
using System.Collections.Generic;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Services
{
    public class CreditCalculatorService : ICreditCalculatorService
    {
        public CreditResponse Calculate(CreditRequest request)
        {
            var response = new CreditResponse();
            var schedule = new List<ScheduleItem>();

            decimal balance = request.Principal;
            int totalMonths = request.TermMonths;

            decimal annualRate = request.AnnualInterestRate / 100m;
            decimal promoAnnualRate = request.PromoAnnualInterestRate / 100m;

            decimal monthlyRate = annualRate / 12m;
            decimal promoMonthlyRate = promoAnnualRate / 12m;

            int graceMonths = request.GraceMonths;
            int promoMonths = request.PromoMonths;

            decimal annuityPayment = 0m;
            decimal principalPerMonth = 0m;

            if (request.PaymentType == PaymentType.Annuity)
            {
                int repaymentMonths = totalMonths - graceMonths;
                if (repaymentMonths > 0)
                {
                    decimal firstRate =
                        promoMonths > graceMonths ? promoMonthlyRate : monthlyRate;

                    annuityPayment = CalculateAnnuity(balance, firstRate, repaymentMonths);
                }
            }
            else if (request.PaymentType == PaymentType.Decreasing)
            {
                int repaymentMonths = totalMonths - graceMonths;
                if (repaymentMonths > 0)
                    principalPerMonth = request.Principal / repaymentMonths;
            }

            for (int month = 1; month <= totalMonths; month++)
            {
                bool isPromo = promoMonths > 0 && month <= promoMonths;
                bool isGrace = graceMonths > 0 && month <= graceMonths;

                decimal rate = isPromo ? promoMonthlyRate : monthlyRate;

                decimal openingBalance = balance;
                decimal interest = openingBalance * rate;
                decimal principal = 0m;
                decimal payment = 0m;

                if (isGrace)
                {
                    principal = 0m;
                    payment = interest;
                }
                else
                {
                    if (request.PaymentType == PaymentType.Annuity &&
                        (month == graceMonths + 1 || month == promoMonths + 1))
                    {
                        int remainingMonths = totalMonths - month + 1;
                        annuityPayment = CalculateAnnuity(balance, rate, remainingMonths);
                    }

                    if (request.PaymentType == PaymentType.Annuity)
                    {
                        principal = annuityPayment - interest;
                        payment = annuityPayment;
                    }
                    else // Decreasing
                    {
                        principal = principalPerMonth;
                        payment = principal + interest;
                    }
                }

                if (month == totalMonths)
                {
                    principal = openingBalance;
                    interest = openingBalance * rate;
                    payment = principal + interest; 
                }

                decimal closingBalance = openingBalance - principal;

                schedule.Add(new ScheduleItem
                {
                    Month = month,
                    OpeningBalance = Round(openingBalance),
                    Interest = Round(interest),
                    Principal = Round(principal),
                    Payment = Round(payment),
                    ClosingBalance = Round(closingBalance)
                });

                balance = closingBalance;
            }
            
            decimal totalInterest = 0m;
            decimal totalPaid = 0m;

            foreach (var row in schedule)
            {
                totalInterest += row.Interest;
                totalPaid += row.Payment;
            }

            response.MonthlyPayment =
                request.PaymentType == PaymentType.Annuity
                    ? Round(annuityPayment)
                    : 0m;

            response.TotalInterest = Round(totalInterest);
            response.TotalPaid = Round(totalPaid);
            response.Schedule = schedule;

            return response;
        }

        private decimal CalculateAnnuity(decimal principal, decimal monthlyRate, int months)
        {
            if (monthlyRate == 0 || months == 0)
                return principal / months;

            return principal * monthlyRate /
                   (1 - (decimal)Math.Pow((double)(1 + monthlyRate), -months));
        }

        private decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
