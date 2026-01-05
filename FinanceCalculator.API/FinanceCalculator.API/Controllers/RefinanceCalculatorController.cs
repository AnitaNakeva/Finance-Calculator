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

        public RefinanceCalculatorController(IRefinanceCalculatorService service)
        {
            _service = service;
        }

        [HttpPost("calculate")]
        public ActionResult<RefinaceResponce> Calculate([FromBody] RefinanceRequest request)
        {
            var result = _service.Calculate(request);
            return Ok(result);
        }
    }
}
