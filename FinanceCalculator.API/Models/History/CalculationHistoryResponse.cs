using System.Collections.Generic;

namespace FinanceCalculator.API.Models
{
    public class CalculationHistoryResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<CalculationHistoryView> Items { get; set; } = new List<CalculationHistoryView>();
    }
}
