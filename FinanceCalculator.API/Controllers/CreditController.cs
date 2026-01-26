using System.Security.Claims;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Route("api/credit")]
    public class CreditController : ControllerBase
    {
        private readonly ICreditCalculatorService _service;
        private readonly IHistoryService _history;

        public CreditController(ICreditCalculatorService service, IHistoryService history)
        {
            _service = service;
            _history = history;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CreditRequest request)
        {
            var result = _service.Calculate(request);

            if (User?.Identity?.IsAuthenticated == true &&
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                await _history.AddRecordAsync(userId, "Credit", request, result);
            }

            return Ok(result);
        }
    }
}
