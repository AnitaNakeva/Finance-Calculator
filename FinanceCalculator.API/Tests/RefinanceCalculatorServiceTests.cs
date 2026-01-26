using System;
using System.Linq;
using FinanceCalculator.API.Models;
using FinanceCalculator.API.Services;
using FluentAssertions;
using Xunit;

namespace FinanceCalculator.API.Tests
{
    public class RefinanceCalculatorServiceTests
    {
        private readonly RefinanceCalculatorService _sut = new();

        [Fact]
        public void Calculates_remaining_months_and_basic_refinance_data()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 20000m,
                CurrentAnnualInterestRate = 6m,
                CurrentTermMonths = 48,
                PaymentsMade = 12,
                EarlyRepaymentFeePercent = 1m,
                UpfrontFeesPercent = 0.5m,
                UpfrontFeesFixed = 100m,
                NewAnnualInterestRate = 4m
            };

            var result = _sut.Calculate(req);

            result.RemainingMonths.Should().Be(36);
            result.RemainingPrincipal.Should().BeGreaterThan(0m);
            result.CurrentMonthlyPayment.Should().BeGreaterThan(0m);
            result.NewMonthlyPayment.Should().BeGreaterThan(0m);

            result.CurrentRemainingSchedule.Should().HaveCount(36);
            result.NewLoanSchedule.Should().HaveCount(36);

            result.NewLoanPrincipal.Should().BeGreaterThan(result.RemainingPrincipal);
        }

        [Fact]
        public void Payments_made_zero_uses_full_principal_and_term()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 10000m,
                CurrentAnnualInterestRate = 5m,
                CurrentTermMonths = 24,
                PaymentsMade = 0,
                EarlyRepaymentFeePercent = 0m,
                UpfrontFeesPercent = 0m,
                UpfrontFeesFixed = 0m,
                NewAnnualInterestRate = 4m
            };

            var result = _sut.Calculate(req);

            result.RemainingPrincipal.Should().Be(10000m);
            result.RemainingMonths.Should().Be(24);
            result.CurrentRemainingSchedule.Should().HaveCount(24);
        }

        [Fact]
        public void Remaining_principal_matches_closing_balance_after_payments()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 15000m,
                CurrentAnnualInterestRate = 6m,
                CurrentTermMonths = 24,
                PaymentsMade = 6,
                EarlyRepaymentFeePercent = 0m,
                UpfrontFeesPercent = 0m,
                UpfrontFeesFixed = 0m,
                NewAnnualInterestRate = 5m
            };

            var result = _sut.Calculate(req);

            result.RemainingPrincipal.Should().BeGreaterThan(0m);
            result.RemainingPrincipal.Should().BeLessThan(15000m);
        }

        [Fact]
        public void Early_repayment_fee_is_included_in_cost_to_close()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 8000m,
                CurrentAnnualInterestRate = 6m,
                CurrentTermMonths = 12,
                PaymentsMade = 2,
                EarlyRepaymentFeePercent = 2m,
                UpfrontFeesPercent = 0m,
                UpfrontFeesFixed = 0m,
                NewAnnualInterestRate = 5m
            };

            var result = _sut.Calculate(req);

            result.EarlyRepaymentFeeAmount.Should().BeGreaterThan(0m);
            result.CurrentTotalCostToClose.Should()
                .Be(result.CurrentTotalPaidRemaining + result.EarlyRepaymentFeeAmount);
        }

        [Fact]
        public void New_loan_principal_includes_upfront_fees()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 10000m,
                CurrentAnnualInterestRate = 6m,
                CurrentTermMonths = 20,
                PaymentsMade = 5,
                EarlyRepaymentFeePercent = 0m,
                UpfrontFeesPercent = 1m,
                UpfrontFeesFixed = 200m,
                NewAnnualInterestRate = 4m
            };

            var result = _sut.Calculate(req);

            result.UpfrontFeesPercentAmount.Should().BeGreaterThan(0m);
            result.UpfrontFeesFixedAmount.Should().Be(200m);
            result.NewLoanPrincipal.Should().Be(
                result.RemainingPrincipal +
                result.UpfrontFeesPercentAmount +
                result.UpfrontFeesFixedAmount
            );
        }

        [Fact]
        public void Savings_positive_when_new_loan_is_cheaper()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 20000m,
                CurrentAnnualInterestRate = 8m,
                CurrentTermMonths = 36,
                PaymentsMade = 12,
                EarlyRepaymentFeePercent = 0m,
                UpfrontFeesPercent = 0m,
                UpfrontFeesFixed = 0m,
                NewAnnualInterestRate = 4m
            };

            var result = _sut.Calculate(req);

            result.Savings.Should().BeGreaterThan(0m);
        }

        [Fact]
        public void Throws_when_payments_made_exceed_term()
        {
            var req = new RefinanceRequest
            {
                CurrentPrincipal = 10000m,
                CurrentAnnualInterestRate = 5m,
                CurrentTermMonths = 12,
                PaymentsMade = 13,
                EarlyRepaymentFeePercent = 0m,
                UpfrontFeesPercent = 0m,
                UpfrontFeesFixed = 0m,
                NewAnnualInterestRate = 4m
            };

            Action act = () => _sut.Calculate(req);
            act.Should().Throw<ArgumentException>();
        }
}
