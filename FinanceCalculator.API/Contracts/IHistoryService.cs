using FinanceCalculator.API.Models;

namespace FinanceCalculator.API.Contracts
{
    public interface IHistoryService
    {
        Task<CalculationHistoryResponse> GetHistoryAsync(int userId, string? calculationType, DateTime? from, DateTime? to, string? search, string sortOrder, int page, int pageSize);
        Task<(byte[] Content, string ContentType, string FileName)?> ExportCsvAsync(int userId, string? calculationType, DateTime? from, DateTime? to, string? search);
        Task<FavoriteCalculation?> FavoriteFromHistoryAsync(int userId, int historyId, string? name);
        Task<CalculationHistoryView?> GetHistoryItemAsync(int userId, int id);
        Task AddRecordAsync(int userId, string calculationType, object request, object response);
    }
}
