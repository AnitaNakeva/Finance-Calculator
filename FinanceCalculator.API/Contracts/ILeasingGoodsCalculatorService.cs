using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface ILeasingGoodsCalculatorService
    {
        LeasingGoodsResponce Calculate(LeasingGoodsRequest request);
    }
}
