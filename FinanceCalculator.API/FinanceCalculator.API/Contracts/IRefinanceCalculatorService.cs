using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface IRefinanceCalculatorService
    {
        RefinaceResponce Calculate(RefinanceRequest request);
    }
}
