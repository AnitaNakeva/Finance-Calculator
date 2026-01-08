using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Services
{
    public class LeasingGoodsCalculatorService : ILeasingGoodsCalculatorService
    {
        public LeasingGoodsResponce Calculate(LeasingGoodsRequest request)
        {
            Validate(request);

            var response = new LeasingGoodsResponce();
            var schedule = new List<ScheduleItem>();

            //  Сума  която реално се финансира
            decimal financedAmount = request.ItemPrice - request.DownPayment;
            response.FinancedAmount = Round(financedAmount);

            //  Първоначална такса (%)
            decimal processingFee =
                financedAmount * (request.ProcessingFeePercent / 100m);

            processingFee = Round(processingFee);
            response.ProcessingFeeAmount = processingFee;

            //  Общо платено по месечни вноски
            decimal totalInstallments =
                request.MonthlyPayment * request.TermMonths;

            //  Общо платено (всичко)
            decimal totalPaid =
                request.DownPayment +
                processingFee +
                totalInstallments;

            response.TotalPaid = Round(totalPaid);

            //  Оскъпяване
            decimal overpayment = totalPaid - request.ItemPrice;
            response.OverpaymentAmount = Round(overpayment);

            response.OverpaymentPercent =
                Round((overpayment / request.ItemPrice) * 100m);

            //  Генериране на график (без лихва, фиксирана вноска)
            decimal balance = financedAmount;

            for (int month = 1; month <= request.TermMonths; month++)
            {
                decimal opening = balance;
                decimal principal = request.MonthlyPayment;

                // Последен месец – корекция
                if (month == request.TermMonths)
                {
                    principal = opening;
                }

                decimal closing = opening - principal;

                schedule.Add(new ScheduleItem
                {
                    Month = month,
                    OpeningBalance = Round(opening),
                    Interest = 0m, // няма лихва
                    Principal = Round(principal),
                    Payment = Round(principal),
                    ClosingBalance = Round(closing)
                });

                balance = closing;
            }

            response.Schedule = schedule;
            return response;
        }

       

        private static void Validate(LeasingGoodsRequest r)
        {
            if (r.ItemPrice <= 0)
                throw new ArgumentException("Цената трябва да е > 0");

            if (r.DownPayment < 0 || r.DownPayment >= r.ItemPrice)
                throw new ArgumentException("Невалидна първоначална вноска");

            if (r.TermMonths <= 0)
                throw new ArgumentException("Срокът трябва да е > 0");

            if (r.MonthlyPayment <= 0)
                throw new ArgumentException("Месечната вноска трябва да е > 0");

            if (r.ProcessingFeePercent < 0)
                throw new ArgumentException("Таксата не може да е отрицателна");
        }

        private static decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
