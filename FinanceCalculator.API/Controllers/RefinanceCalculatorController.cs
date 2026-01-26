using System.Security.Claims;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Route("api/refinance")]
    public class RefinanceCalculatorController : ControllerBase
    {
        private readonly IRefinanceCalculatorService _service;
        private readonly IHistoryService _history;

        public RefinanceCalculatorController(IRefinanceCalculatorService service, IHistoryService history)
        {
            _service = service;
            _history = history;
        }

        [HttpPost("calculate")]
        public async Task<ActionResult<RefinaceResponce>> Calculate([FromBody] RefinanceRequest request)
        {
            var result = _service.Calculate(request);

            if (User?.Identity?.IsAuthenticated == true &&
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                await _history.AddRecordAsync(userId, "Refinance", request, result);
            }

            return Ok(result);
        }
    }
}
