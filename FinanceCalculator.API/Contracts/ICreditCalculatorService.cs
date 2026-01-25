using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface ICreditCalculatorService
    {
        CreditResponse Calculate(CreditRequest request);
    }
}
