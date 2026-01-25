using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Services
{
    public class RefinanceCalculatorService : IRefinanceCalculatorService
    {
        public RefinaceResponce Calculate (RefinanceRequest request)
        {
            //Валидация на входните данни

            Validate(request);

            var response = new RefinaceResponce();

            //Колко месеца остават по текущия кредит

            int remainingMonths = request.CurrentTermMonths - request.PaymentsMade;
            response.RemainingMonths = remainingMonths;

            // Изчисляваме целия погасителен план на текущия кредит

            var fullCurrentSchedule = BuildAnnuitySchedule(
                principal: request.CurrentPrincipal,
                annualInterestRatePercent: request.CurrentAnnualInterestRate,
                termMonths: request.CurrentTermMonths
            );

            // Определяме колко точно главница остава

            decimal remainingPrincipal;

            if (request.PaymentsMade == 0)
            {
                // Ако няма направени вноски - дължим цялата сума
                remainingPrincipal = request.CurrentPrincipal;
            }
            else
            {
                // Вземаме closing balance след последната платена вноска
                remainingPrincipal =
                    fullCurrentSchedule[request.PaymentsMade - 1].ClosingBalance;
            }

            remainingPrincipal = Round(remainingPrincipal);
            response.RemainingPrincipal = remainingPrincipal;

            // Месечната вноска по текущия кредит

            decimal currentMonthlyPayment =
                fullCurrentSchedule.Count > 0
                    ? fullCurrentSchedule[0].Payment
                    : 0m;

            response.CurrentMonthlyPayment = Round(currentMonthlyPayment);

            // Създаваме погасителен план само за оставащите месеци

            var currentRemainingSchedule = new List<ScheduleItem>();

            if (remainingMonths > 0)
            {
                currentRemainingSchedule = BuildAnnuitySchedule(
                    principal: remainingPrincipal,
                    annualInterestRatePercent: request.CurrentAnnualInterestRate,
                    termMonths: remainingMonths
                );
            }

            response.CurrentRemainingSchedule = currentRemainingSchedule;

            // Колко пари ще платим, ако не рефинансираме
            decimal currentTotalPaidRemaining = 0m;

            foreach (var row in currentRemainingSchedule)
            {
                currentTotalPaidRemaining += row.Payment;
            }

            response.CurrentTotalPaidRemaining =
                Round(currentTotalPaidRemaining);

            // Такса за предсрочно погасяване

            decimal earlyRepaymentFee =
                remainingPrincipal *
                (request.EarlyRepaymentFeePercent / 100m);

            earlyRepaymentFee = Round(earlyRepaymentFee);
            response.EarlyRepaymentFeeAmount = earlyRepaymentFee;

            // Обща цена, ако затворим стария кредит
            decimal currentTotalCostToClose =
                currentTotalPaidRemaining + earlyRepaymentFee;

            response.CurrentTotalCostToClose =
                Round(currentTotalCostToClose);

            // Нов кредит (рефинансиране)

            decimal upfrontFeesPercentAmount =
                remainingPrincipal *
                (request.UpfrontFeesPercent / 100m);

            upfrontFeesPercentAmount = Round(upfrontFeesPercentAmount);

            decimal upfrontFeesFixed =
                Round(request.UpfrontFeesFixed);

            decimal newLoanPrincipal =
                remainingPrincipal +
                upfrontFeesPercentAmount +
                upfrontFeesFixed;

            newLoanPrincipal = Round(newLoanPrincipal);

            response.UpfrontFeesPercentAmount = upfrontFeesPercentAmount;
            response.UpfrontFeesFixedAmount = upfrontFeesFixed;
            response.NewLoanPrincipal = newLoanPrincipal;

            // Погасителен план на новия кредит
            var newLoanSchedule = BuildAnnuitySchedule(
                principal: newLoanPrincipal,
                annualInterestRatePercent: request.NewAnnualInterestRate,
                termMonths: remainingMonths
            );

            response.NewLoanSchedule = newLoanSchedule;

            //Колко ще платим общо по новия кредит
            decimal newTotalPaid = 0m;

            foreach (var row in newLoanSchedule)
            {
                newTotalPaid += row.Payment;
            }

            response.NewTotalPaid = Round(newTotalPaid);

            // Месечната вноска по новия кредит
            response.NewMonthlyPayment =
                newLoanSchedule.Count > 0
                    ? Round(newLoanSchedule[0].Payment)
                    : 0m;

            // Реалната печалба/загуба от рефинансирането
            response.Savings =
                Round(currentTotalCostToClose - newTotalPaid);

            return response;
        }



        // Генерира анюитетен погасителен план
        private static List<ScheduleItem> BuildAnnuitySchedule(
            decimal principal,
            decimal annualInterestRatePercent,
            int termMonths)
        {
            var schedule = new List<ScheduleItem>();

            decimal annualRate = annualInterestRatePercent / 100m;
            decimal monthlyRate = annualRate / 12m;

            decimal balance = principal;

            // Формула за анюитетна вноска
            decimal annuityPayment =
                CalculateAnnuity(balance, monthlyRate, termMonths);

            for (int month = 1; month <= termMonths; month++)
            {
                decimal opening = balance;
                decimal interest = opening * monthlyRate;
                decimal principalPart = annuityPayment - interest;

                // Корекция за последния месец
                if (month == termMonths)
                {
                    principalPart = opening;
                    interest = opening * monthlyRate;
                    annuityPayment = principalPart + interest;
                }

                decimal closing = opening - principalPart;

                schedule.Add(new ScheduleItem
                {
                    Month = month,
                    OpeningBalance = Round(opening),
                    Interest = Round(interest),
                    Principal = Round(principalPart),
                    Payment = Round(annuityPayment),
                    ClosingBalance = Round(closing)
                });

                balance = closing;
            }

            return schedule;
        }

        // Формула за анюитет
        private static decimal CalculateAnnuity(
            decimal principal,
            decimal monthlyRate,
            int months)
        {
            if (monthlyRate == 0m)
                return principal / months;

            return principal * monthlyRate /
                   (1 - (decimal)Math.Pow((double)(1 + monthlyRate), -months));
        }

        // Закръгляне до 2 знака
        private static decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        // Проверка за грешни входни данни
        private static void Validate(RefinanceRequest r)
        {
            if (r.CurrentPrincipal <= 0)
                throw new ArgumentException("Размерът на кредита трябва да е > 0");

            if (r.CurrentTermMonths <= 0)
                throw new ArgumentException("Срокът трябва да е > 0");

            if (r.PaymentsMade < 0 || r.PaymentsMade > r.CurrentTermMonths)
                throw new ArgumentException("Невалиден брой направени вноски");
        }
    }
}

