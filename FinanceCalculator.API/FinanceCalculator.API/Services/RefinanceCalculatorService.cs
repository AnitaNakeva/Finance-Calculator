using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Services
{
    public class RefinanceCalculatorService : IRefinanceCalculatorService
    {
        public RefinaceResponce Calculate (RefinanceRequest request)
        {
            // 1️⃣ Валидация на входните данни
            // Проверяваме дали има логически смисъл това, което ни е подадено
            Validate(request);

            var response = new RefinaceResponce();

            // 2️⃣ Колко месеца остават по текущия кредит
            // общ срок - направени вноски
            int remainingMonths = request.CurrentTermMonths - request.PaymentsMade;
            response.RemainingMonths = remainingMonths;

            // 3️⃣ Изчисляваме ЦЕЛИЯ погасителен план на текущия кредит
            // (както при стандартен кредитен калкулатор)
            var fullCurrentSchedule = BuildAnnuitySchedule(
                principal: request.CurrentPrincipal,
                annualInterestRatePercent: request.CurrentAnnualInterestRate,
                termMonths: request.CurrentTermMonths
            );

            // 4️⃣ Определяме колко точно главница остава
            // след като вече са направени N вноски
            decimal remainingPrincipal;

            if (request.PaymentsMade == 0)
            {
                // Ако няма направени вноски → дължим цялата сума
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

            // 5️⃣ Месечната вноска по текущия кредит
            // (анюитетна – една и съща всеки месец)
            decimal currentMonthlyPayment =
                fullCurrentSchedule.Count > 0
                    ? fullCurrentSchedule[0].Payment
                    : 0m;

            response.CurrentMonthlyPayment = Round(currentMonthlyPayment);

            // 6️⃣ Създаваме погасителен план САМО за оставащите месеци
            // Тук вече работим с остатъчната главница
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

            // 7️⃣ Колко пари ще платим, ако НЕ рефинансираме
            decimal currentTotalPaidRemaining = 0m;

            foreach (var row in currentRemainingSchedule)
            {
                currentTotalPaidRemaining += row.Payment;
            }

            response.CurrentTotalPaidRemaining =
                Round(currentTotalPaidRemaining);

            // 8️⃣ Такса за предсрочно погасяване
            // процент от оставащата главница
            decimal earlyRepaymentFee =
                remainingPrincipal *
                (request.EarlyRepaymentFeePercent / 100m);

            earlyRepaymentFee = Round(earlyRepaymentFee);
            response.EarlyRepaymentFeeAmount = earlyRepaymentFee;

            // 9️⃣ Обща цена, ако затворим стария кредит
            decimal currentTotalCostToClose =
                currentTotalPaidRemaining + earlyRepaymentFee;

            response.CurrentTotalCostToClose =
                Round(currentTotalCostToClose);

            // 🔟 Нов кредит (рефинансиране)
            // главница = оставаща главница + първоначални такси
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

            // 1️⃣1️⃣ Погасителен план на новия кредит
            var newLoanSchedule = BuildAnnuitySchedule(
                principal: newLoanPrincipal,
                annualInterestRatePercent: request.NewAnnualInterestRate,
                termMonths: remainingMonths
            );

            response.NewLoanSchedule = newLoanSchedule;

            // 1️⃣2️⃣ Колко ще платим ОБЩО по новия кредит
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

            // 1️⃣3️⃣ Реалната печалба/загуба от рефинансирането
            response.Savings =
                Round(currentTotalCostToClose - newTotalPaid);

            return response;
        }

        // ==========================================
        // Помощни методи
        // ==========================================

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
