using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using FinanceCalculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Route("api/credit")]
    public class CreditController : ControllerBase
    {
        private readonly ICreditCalculatorService _service;

        public CreditController(ICreditCalculatorService service)
        {
            _service = service;
        }

        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] CreditRequest request)
        {
            var result = _service.Calculate(request);
            return Ok(result);
        }
    }
}
