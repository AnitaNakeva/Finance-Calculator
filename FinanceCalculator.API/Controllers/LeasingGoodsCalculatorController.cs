using System.Security.Claims;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Route("api/leasing-goods")]
    public class LeasingGoodsCalculatorController : ControllerBase
    {
        private readonly ILeasingGoodsCalculatorService _service;
        private readonly IHistoryService _history;

        public LeasingGoodsCalculatorController(ILeasingGoodsCalculatorService service, IHistoryService history)
        {
            _service = service;
            _history = history;
        }

        [HttpPost("calculate")]
        public async Task<ActionResult<LeasingGoodsResponce>> Calculate([FromBody] LeasingGoodsRequest request)
        {
            var result = _service.Calculate(request);

            if (User?.Identity?.IsAuthenticated == true &&
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                await _history.AddRecordAsync(userId, "LeasingGoods", request, result);
            }

            return Ok(result);
        }
    }
}
