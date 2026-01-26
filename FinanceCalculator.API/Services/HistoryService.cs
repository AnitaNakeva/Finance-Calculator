using System.Globalization;
using System.Text;
using System.Text.Json;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Data;
using FinanceCalculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalculator.API.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AppDbContext _db;

        public HistoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CalculationHistoryResponse> GetHistoryAsync(int userId, string? calculationType, DateTime? from, DateTime? to, string? search, string sortOrder, int page, int pageSize)
        {
            var query = BuildQuery(userId, calculationType, from, to, search);

            var total = await query.CountAsync();

            sortOrder = sortOrder.ToLowerInvariant();
            query = sortOrder == "asc"
                ? query.OrderBy(r => r.CreatedAtUtc)
                : query.OrderByDescending(r => r.CreatedAtUtc);

            var skip = (page - 1) * pageSize;

            var rawItems = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(r => new CalculationHistoryItem
                {
                    Id = r.Id,
                    CalculationType = r.CalculationType,
                    RequestJson = r.RequestJson,
                    ResponseJson = r.ResponseJson,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();

            var views = rawItems.Select(ToView).ToList();

            return new CalculationHistoryResponse
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = views
            };
        }

        public async Task<(byte[] Content, string ContentType, string FileName)?> ExportCsvAsync(int userId, string? calculationType, DateTime? from, DateTime? to, string? search)
        {
            var query = BuildQuery(userId, calculationType, from, to, search)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(5000);

            var rows = await query
                .Select(r => new CalculationHistoryItem
                {
                    Id = r.Id,
                    CalculationType = r.CalculationType,
                    RequestJson = r.RequestJson,
                    ResponseJson = r.ResponseJson,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,CalculationType,CreatedAtUtc,Principal,TermMonths,AnnualRate,PaymentType,MonthlyPayment,TotalPaid,TotalInterest,InitialFees,MonthlyFees,AnnualFees,TotalFees,AnnualPercentageRate");

            foreach (var r in rows)
            {
                var parsed = ParseDetails(r);
                sb.AppendLine(string.Join(",", new[]
                {
                    CsvEscape(r.Id.ToString()),
                    CsvEscape(r.CalculationType),
                    CsvEscape(r.CreatedAtUtc.ToString("o", CultureInfo.InvariantCulture)),
                    CsvEscape(parsed.Principal?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.TermMonths?.ToString() ?? string.Empty),
                    CsvEscape(parsed.AnnualRate?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.PaymentType ?? string.Empty),
                    CsvEscape(parsed.MonthlyPayment ?? string.Empty),
                    CsvEscape(parsed.TotalPaid?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.TotalInterest?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.InitialFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.MonthlyFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.AnnualFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.TotalFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(parsed.AnnualPercentageRate?.ToString("0.##") ?? string.Empty)
                }));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return (bytes, "text/csv", "calculation-history.csv");
        }

        public async Task<FavoriteCalculation?> FavoriteFromHistoryAsync(int userId, int historyId, string? name)
        {
            var record = await _db.CalculationRecords.FirstOrDefaultAsync(r => r.Id == historyId && r.UserId == userId);
            if (record == null) return null;

            var favoriteName = string.IsNullOrWhiteSpace(name)
                ? $"Favorite {record.CalculationType} #{record.Id}"
                : name.Trim();

            var exists = await _db.FavoriteCalculations.AnyAsync(f => f.UserId == userId && f.Name == favoriteName);
            if (exists) return null;

            var favorite = new FavoriteCalculation
            {
                UserId = userId,
                Name = favoriteName,
                CalculationType = record.CalculationType,
                RequestJson = record.RequestJson,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.FavoriteCalculations.Add(favorite);
            await _db.SaveChangesAsync();
            return favorite;
        }

        public async Task AddRecordAsync(int userId, string calculationType, object request, object response)
        {
            var record = new CalculationRecord
            {
                UserId = userId,
                CalculationType = calculationType,
                RequestJson = JsonSerializer.Serialize(request),
                ResponseJson = JsonSerializer.Serialize(response),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.CalculationRecords.Add(record);
            await _db.SaveChangesAsync();
        }

        public async Task<CalculationHistoryView?> GetHistoryItemAsync(int userId, int id)
        {
            var record = await _db.CalculationRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (record == null) return null;

            var item = new CalculationHistoryItem
            {
                Id = record.Id,
                CalculationType = record.CalculationType,
                RequestJson = record.RequestJson,
                ResponseJson = record.ResponseJson,
                CreatedAtUtc = record.CreatedAtUtc
            };

            return ToView(item);
        }

        private IQueryable<CalculationRecord> BuildQuery(int userId, string? calculationType, DateTime? from, DateTime? to, string? search)
        {
            var query = _db.CalculationRecords.Where(r => r.UserId == userId);

            if (!string.IsNullOrWhiteSpace(calculationType))
            {
                var type = calculationType.Trim().ToLowerInvariant();
                query = query.Where(r => r.CalculationType.ToLower() == type);
            }

            if (from.HasValue)
            {
                query = query.Where(r => r.CreatedAtUtc >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.CreatedAtUtc <= to.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search.Trim().ToLowerInvariant()}%";
                query = query.Where(r =>
                    EF.Functions.Like(r.RequestJson.ToLower(), term) ||
                    EF.Functions.Like(r.ResponseJson.ToLower(), term));
            }

            return query;
        }

        private CalculationHistoryView ToView(CalculationHistoryItem item)
        {
            var parsed = ParseDetails(item);
            return new CalculationHistoryView
            {
                Id = item.Id,
                CalculationType = item.CalculationType,
                CreatedAtUtc = item.CreatedAtUtc,
                Principal = parsed.Principal,
                TermMonths = parsed.TermMonths,
                AnnualRate = parsed.AnnualRate,
                PaymentType = parsed.PaymentType,
                MonthlyPayment = parsed.MonthlyPayment,
                TotalPaid = parsed.TotalPaid,
                TotalInterest = parsed.TotalInterest,
                FinancedAmount = parsed.FinancedAmount,
                OverpaymentPercent = parsed.OverpaymentPercent,
                Savings = parsed.Savings,
                CurrentCloseCost = parsed.CurrentCloseCost,
                InitialFees = parsed.InitialFees,
                MonthlyFees = parsed.MonthlyFees,
                AnnualFees = parsed.AnnualFees,
                TotalFees = parsed.TotalFees,
                AnnualPercentageRate = parsed.AnnualPercentageRate
            };
        }

        private ParsedCalculation ParseDetails(CalculationHistoryItem item)
        {
            var parsed = new ParsedCalculation { CalculationType = item.CalculationType, CreatedAtUtc = item.CreatedAtUtc };
            try
            {
                switch (item.CalculationType.ToLowerInvariant())
                {
                    case "credit":
                        var creditRequest = JsonSerializer.Deserialize<CreditRequest>(item.RequestJson);
                        var creditResponse = JsonSerializer.Deserialize<CreditResponse>(item.ResponseJson);
                        if (creditRequest != null && creditResponse != null)
                        {
                            parsed.Principal = creditRequest.Principal;
                            parsed.TermMonths = creditRequest.TermMonths;
                            parsed.AnnualRate = creditRequest.AnnualInterestRate;
                            parsed.PaymentType = creditRequest.PaymentType == PaymentType.Decreasing ? "Decreasing" : "Annuity";
                            if (creditRequest.PaymentType == PaymentType.Decreasing && creditResponse.Schedule != null && creditResponse.Schedule.Count > 0)
                            {
                                var avg = creditResponse.Schedule.Average(s => s.Payment);
                                parsed.MonthlyPayment = $"Average {avg:0.##}";
                            }
                            else
                            {
                                parsed.MonthlyPayment = creditResponse.MonthlyPayment.ToString("0.##");
                            }
                            parsed.TotalPaid = creditResponse.TotalPaid;
                            parsed.TotalInterest = creditResponse.TotalInterest;
                            parsed.InitialFees = creditResponse.InitialFeesTotal;
                            parsed.MonthlyFees = creditResponse.MonthlyFeesTotal;
                            parsed.AnnualFees = creditResponse.AnnualFeesTotal;
                            parsed.TotalFees = creditResponse.TotalFees;
                            parsed.AnnualPercentageRate = creditResponse.AnnualPercentageRate;
                        }
                        break;
                    case "leasinggoods":
                        var leaseResponse = JsonSerializer.Deserialize<LeasingGoodsResponce>(item.ResponseJson);
                        if (leaseResponse != null)
                        {
                            parsed.FinancedAmount = leaseResponse.FinancedAmount;
                            parsed.TotalPaid = leaseResponse.TotalPaid;
                            parsed.OverpaymentPercent = leaseResponse.OverpaymentPercent;
                        }
                        break;
                    case "refinance":
                        var refiResponse = JsonSerializer.Deserialize<RefinaceResponce>(item.ResponseJson);
                        if (refiResponse != null)
                        {
                            parsed.MonthlyPayment = refiResponse.NewMonthlyPayment.ToString("0.##");
                            parsed.Savings = refiResponse.Savings;
                            parsed.CurrentCloseCost = refiResponse.CurrentTotalCostToClose;
                        }
                        break;
                }
            }
            catch
            {
                // ignore parsing errors
            }

            return parsed;
        }

        private string CsvEscape(string input)
        {
            if (input == null) return string.Empty;
            var needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r');
            var value = input.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{value}\"" : value;
        }
    }
}
