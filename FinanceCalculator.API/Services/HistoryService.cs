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

            var records = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var views = records.Select(ToView).ToList();

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

            var rows = await query.ToListAsync();

            // because string is immutable, with sb we don't create a new string every time we change it
            var sb = new StringBuilder();
            
            sb.AppendLine("Id,CalculationType,CreatedAtUtc,Principal,TermMonths,AnnualRate,PaymentType,MonthlyPayment,TotalPaid,TotalInterest,InitialFees,MonthlyFees,AnnualFees,TotalFees,AnnualPercentageRate");

            foreach (var record in rows)
            {
                var view = ToView(record);
                sb.AppendLine(string.Join(",", new[]
                {
                    CsvEscape(record.Id.ToString()),
                    CsvEscape(record.CalculationType),
                    CsvEscape(record.CreatedAtUtc.ToString("o", CultureInfo.InvariantCulture)),
                    CsvEscape(view.Principal?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.TermMonths?.ToString() ?? string.Empty),
                    CsvEscape(view.AnnualRate?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.PaymentType ?? string.Empty),
                    CsvEscape(view.MonthlyPayment ?? string.Empty),
                    CsvEscape(view.TotalPaid?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.TotalInterest?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.InitialFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.MonthlyFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.AnnualFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.TotalFees?.ToString("0.##") ?? string.Empty),
                    CsvEscape(view.AnnualPercentageRate?.ToString("0.##") ?? string.Empty)
                }));
            }

            // transform it to a byte array
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

            return ToView(record);
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
                // search everywhere in the text
                var term = $"%{search.Trim().ToLowerInvariant()}%";
                query = query.Where(r =>
                    // to make SQL LIKE query
                    EF.Functions.Like(r.RequestJson.ToLower(), term) ||
                    EF.Functions.Like(r.ResponseJson.ToLower(), term));
            }

            return query;
        }

        private CalculationHistoryView ToView(CalculationRecord record)
        {
            var view = new CalculationHistoryView
            {
                Id = record.Id,
                CalculationType = record.CalculationType,
                CreatedAtUtc = record.CreatedAtUtc
            };

            try
            {
                switch (record.CalculationType.ToLowerInvariant())
                {
                    case "credit":
                        var creditRequest = JsonSerializer.Deserialize<CreditRequest>(record.RequestJson);
                        var creditResponse = JsonSerializer.Deserialize<CreditResponse>(record.ResponseJson);
                        if (creditRequest != null && creditResponse != null)
                        {
                            view.Principal = creditRequest.Principal;
                            view.TermMonths = creditRequest.TermMonths;
                            view.AnnualRate = creditRequest.AnnualInterestRate;
                            view.PaymentType = creditRequest.PaymentType == PaymentType.Decreasing ? "Decreasing" : "Annuity";

                            if (creditRequest.PaymentType == PaymentType.Decreasing &&
                                creditResponse.Schedule != null && creditResponse.Schedule.Count > 0)
                            {
                                var avg = creditResponse.Schedule.Average(s => s.Payment);
                                view.MonthlyPayment = $"Average {avg:0.##}";
                            }
                            else
                            {
                                view.MonthlyPayment = creditResponse.MonthlyPayment.ToString("0.##");
                            }

                            view.TotalPaid = creditResponse.TotalPaid;
                            view.TotalInterest = creditResponse.TotalInterest;
                            view.InitialFees = creditResponse.InitialFeesTotal;
                            view.MonthlyFees = creditResponse.MonthlyFeesTotal;
                            view.AnnualFees = creditResponse.AnnualFeesTotal;
                            view.TotalFees = creditResponse.TotalFees;
                            view.AnnualPercentageRate = creditResponse.AnnualPercentageRate;
                        }
                        break;

                    case "leasinggoods":
                        var leaseResponse = JsonSerializer.Deserialize<LeasingGoodsResponce>(record.ResponseJson);
                        if (leaseResponse != null)
                        {
                            view.FinancedAmount = leaseResponse.FinancedAmount;
                            view.TotalPaid = leaseResponse.TotalPaid;
                            view.OverpaymentPercent = leaseResponse.OverpaymentPercent;
                        }
                        break;

                    case "refinance":
                        var refiResponse = JsonSerializer.Deserialize<RefinaceResponce>(record.ResponseJson);
                        if (refiResponse != null)
                        {
                            view.MonthlyPayment = refiResponse.NewMonthlyPayment.ToString("0.##");
                            view.Savings = refiResponse.Savings;
                            view.CurrentCloseCost = refiResponse.CurrentTotalCostToClose;
                        }
                        break;
                }
            }
            catch
            {
                // ignore parse issues for legacy records
            }

            return view;
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
