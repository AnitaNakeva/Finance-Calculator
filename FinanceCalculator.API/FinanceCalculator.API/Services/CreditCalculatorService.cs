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

            decimal monthlyRate = request.AnnualInterestRate / 12m;
            decimal promoMonthlyRate = request.PromoAnnualInterestRate / 12m;

            decimal annuityPayment = 0m;

            // 1️⃣ АНЮИТЕТ – първоначална вноска (винаги за ЦЕЛИЯ срок)
            if (request.PaymentType == PaymentType.Annuity)
            {
                decimal firstRate = request.PromoMonths > 0
                    ? promoMonthlyRate
                    : monthlyRate;

                annuityPayment = CalculateAnnuity(balance, firstRate, totalMonths);
                response.MonthlyPayment = Round(annuityPayment);
            }

            // 2️⃣ НАМАЛЯВАЩА СХЕМА – фиксирана главница
            decimal principalPerMonth =
                request.PaymentType == PaymentType.Decreasing
                    ? request.Principal / totalMonths
                    : 0m;

            // 3️⃣ МЕСЕЧЕН ЦИКЪЛ
            for (int month = 1; month <= totalMonths; month++)
            {
                decimal rate =
                    request.PromoMonths > 0 && month <= request.PromoMonths
                        ? promoMonthlyRate
                        : monthlyRate;

                // ➤ преизчисляване на анюитет след промо периода
                if (request.PaymentType == PaymentType.Annuity &&
                    request.PromoMonths > 0 &&
                    month == request.PromoMonths + 1)
                {
                    int remainingMonths = totalMonths - request.PromoMonths;
                    annuityPayment = CalculateAnnuity(balance, monthlyRate, remainingMonths);
                }

                decimal openingBalance = balance;
                decimal interest = openingBalance * rate;
                decimal principal;
                decimal payment;

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

                // ➤ последен месец – затваряме кредита точно
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

            // 4️⃣ ОБЩИ СУМИ
            decimal totalInterest = 0m;
            decimal totalPaid = 0m;

            foreach (var row in schedule)
            {
                totalInterest += row.Interest;
                totalPaid += row.Payment;
            }

            response.TotalInterest = Round(totalInterest);
            response.TotalPaid = Round(totalPaid);
            response.Schedule = schedule;

            return response;
        }

        // 🔢 Формула за анюитетна вноска
        private decimal CalculateAnnuity(decimal principal, decimal monthlyRate, int months)
        {
            if (monthlyRate == 0 || months == 0)
                return principal / months;

            return principal * monthlyRate /
                   (1 - (decimal)Math.Pow((double)(1 + monthlyRate), -months));
        }

        // 🔁 Закръгляне до 2 знака
        private decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
