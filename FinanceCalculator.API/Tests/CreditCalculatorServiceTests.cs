using System;
using System.Linq;
using FinanceCalculator.API.Models;
using FinanceCalculator.API.Services;
using FluentAssertions;
using Xunit;

namespace FinanceCalculator.API.Tests
{
    public class CreditCalculatorServiceTests
    {
        private readonly CreditCalculatorService _sut = new();

        [Fact]
        public void Annuity_basic_values_should_match_formula()
        {
            var req = new CreditRequest
            {
                Principal = 12000m,
                TermMonths = 12,
                AnnualInterestRate = 12m,
                PaymentType = PaymentType.Annuity,
                GraceMonths = 0,
                PromoMonths = 0,
                PromoAnnualInterestRate = 0
            };

            var result = _sut.Calculate(req);

            // expected annuity payment: P * r / (1 - (1+r)^-n)
            var monthlyRate = 0.12m / 12m;
            var expectedPayment = req.Principal * monthlyRate / (1 - (decimal)Math.Pow((double)(1 + monthlyRate), -req.TermMonths));

            result.MonthlyPayment.Should().BeApproximately(expectedPayment, 0.01m);
            result.Schedule.Should().HaveCount(12);
            result.Schedule.Last().ClosingBalance.Should().Be(0);
            result.TotalPaid.Should().BeApproximately(result.Schedule.Sum(s => s.Payment), 0.01m);
        }

        [Fact]
        public void Decreasing_should_have_descending_payments_and_average_exposed()
        {
            var req = new CreditRequest
            {
                Principal = 10000m,
                TermMonths = 10,
                AnnualInterestRate = 6m,
                PaymentType = PaymentType.Decreasing,
                GraceMonths = 0,
                PromoMonths = 0,
                PromoAnnualInterestRate = 0
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(10);
            var payments = result.Schedule.Select(s => s.Payment).ToList();
            payments.Should().BeInDescendingOrder();
            payments.Last().Should().BeGreaterThan(0);
            var avg = payments.Average();
            avg.Should().BeGreaterThan(payments.Last());
            result.TotalPaid.Should().BeApproximately(payments.Sum(), 0.01m);
        }

        [Fact]
        public void Annuity_with_promo_and_grace_recalculates_payment_after_periods()
        {
            var req = new CreditRequest
            {
                Principal = 5000m,
                TermMonths = 12,
                AnnualInterestRate = 10m,
                PromoAnnualInterestRate = 5m,
                PromoMonths = 2,
                GraceMonths = 1,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(12);
            // First month: grace -> payment should equal interest only at promo rate
            var m1 = result.Schedule[0];
            m1.Principal.Should().Be(0);
            m1.Payment.Should().BeGreaterThan(0);
            // Month 2 still promo but no grace
            var m2 = result.Schedule[1];
            m2.Payment.Should().BeGreaterThan(m1.Payment);
            // After promo, payment increases
            var m3 = result.Schedule[2];
            m3.Payment.Should().BeGreaterThan(m2.Payment);
        }

        [Fact]
        public void Zero_interest_annuity_behaves_as_simple_division()
        {
            var req = new CreditRequest
            {
                Principal = 1200m,
                TermMonths = 12,
                AnnualInterestRate = 0m,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);
            result.MonthlyPayment.Should().Be(100m);
            result.TotalInterest.Should().Be(0m);
            result.Schedule.All(s => s.Interest == 0m).Should().BeTrue();
        }
        
        [Fact]
        public void One_month_credit_should_close_correctly()
        {
            var req = new CreditRequest
            {
                Principal = 1000m,
                TermMonths = 1,
                AnnualInterestRate = 12m,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(1);
            result.Schedule[0].ClosingBalance.Should().Be(0);
        }

        [Fact]
        public void Full_grace_period_should_not_reduce_principal()
        {
            var req = new CreditRequest
            {
                Principal = 1000m,
                TermMonths = 6,
                AnnualInterestRate = 12m,
                GraceMonths = 6,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);

            result.Schedule.All(s => s.Principal == 0).Should().BeTrue();
        }

        [Fact]
        public void Promo_longer_than_term_should_not_break()
        {
            var req = new CreditRequest
            {
                Principal = 1000m,
                TermMonths = 6,
                AnnualInterestRate = 10m,
                PromoAnnualInterestRate = 5m,
                PromoMonths = 12,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(6);
        }

        [Fact]
        public void Grace_longer_than_term_should_not_break()
        {
            var req = new CreditRequest
            {
                Principal = 1000m,
                TermMonths = 6,
                AnnualInterestRate = 10m,
                GraceMonths = 12,
                PaymentType = PaymentType.Annuity
            };

            var result = _sut.Calculate(req);

            result.Schedule.Should().HaveCount(6);
            result.Schedule.All(s => s.Principal == 0).Should().BeTrue();
        }

        [Fact]
        public void Decreasing_should_have_constant_principal()
        {
            var req = new CreditRequest
            {
                Principal = 12000m,
                TermMonths = 12,
                AnnualInterestRate = 12m,
                PaymentType = PaymentType.Decreasing
            };

            var result = _sut.Calculate(req);

            var principals = result.Schedule.Select(s => s.Principal).ToList();
            principals.Distinct().Count().Should().Be(1);
        }

    }
}
