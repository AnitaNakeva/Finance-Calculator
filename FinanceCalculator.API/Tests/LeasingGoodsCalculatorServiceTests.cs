using System;
using System.Linq;
using FinanceCalculator.API.Models;
using FinanceCalculator.API.Services;
using FluentAssertions;
using Xunit;

namespace FinanceCalculator.API.Tests
{
    public class LeasingGoodsCalculatorServiceTests
    {
        private readonly LeasingGoodsCalculatorService _sut = new();

        [Fact]
        public void Calculates_totals_and_overpayment_correctly()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 10000m,
                DownPayment = 2000m,
                TermMonths = 8,
                MonthlyPayment = 1000m,
                ProcessingFeePercent = 1m
            };

            var result = _sut.Calculate(req);

            result.FinancedAmount.Should().Be(8000m);
            result.ProcessingFeeAmount.Should().Be(80m);
            result.TotalPaid.Should().Be(2000m + 80m + 8000m);
            result.OverpaymentAmount.Should().Be(result.TotalPaid - req.ItemPrice);
            result.OverpaymentPercent.Should().BeApproximately(
                (result.OverpaymentAmount / req.ItemPrice) * 100m, 0.01m);

            result.Schedule.Should().HaveCount(8);
            result.Schedule.Last().ClosingBalance.Should().Be(0m);
        }

        [Fact]
        public void Schedule_should_have_zero_interest_and_fixed_principal()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 6000m,
                DownPayment = 0m,
                TermMonths = 6,
                MonthlyPayment = 1000m,
                ProcessingFeePercent = 0m
            };

            var result = _sut.Calculate(req);

            result.Schedule.All(s => s.Interest == 0m).Should().BeTrue();
            result.Schedule.Take(5).All(s => s.Principal == 1000m).Should().BeTrue();
        }

        [Fact]
        public void Last_payment_adjusts_to_remaining_balance()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 5000m,
                DownPayment = 0m,
                TermMonths = 3,
                MonthlyPayment = 1666.67m,
                ProcessingFeePercent = 0m
            };

            var result = _sut.Calculate(req);

            var last = result.Schedule.Last();
            last.Principal.Should().BeApproximately(1666.66m, 0.02m);
            last.Payment.Should().Be(last.Principal);
            last.ClosingBalance.Should().Be(0m);
        }

        [Fact]
        public void Throws_when_downpayment_is_invalid()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 10000m,
                DownPayment = 10000m,
                TermMonths = 10,
                MonthlyPayment = 1000m,
                ProcessingFeePercent = 0m
            };

            Action act = () => _sut.Calculate(req);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Throws_when_monthly_payment_is_too_small_to_cover_amount()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 10000m,
                DownPayment = 0m,
                TermMonths = 10,
                MonthlyPayment = 500m,
                ProcessingFeePercent = 0m
            };

            Action act = () => _sut.Calculate(req);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Zero_processing_fee_should_work()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 3000m,
                DownPayment = 1000m,
                TermMonths = 4,
                MonthlyPayment = 500m,
                ProcessingFeePercent = 0m
            };

            var result = _sut.Calculate(req);

            result.ProcessingFeeAmount.Should().Be(0m);
            result.TotalPaid.Should().Be(1000m + 2000m);
        }

        [Fact]
        public void One_month_leasing_should_close_correctly()
        {
            var req = new LeasingGoodsRequest
            {
                ItemPrice = 1200m,
                DownPayment = 200m,
                TermMonths = 1,
                MonthlyPayment = 1000m,
                ProcessingFeePercent = 0m
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(1);
            result.Schedule[0].ClosingBalance.Should().Be(0m);
        }
    }
}
