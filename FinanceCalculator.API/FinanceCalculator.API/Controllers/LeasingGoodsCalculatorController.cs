using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{

    [ApiController]
    [Route("api/leasing-goods")]
    public class LeasingGoodsCalculatorController :ControllerBase
    {
        private readonly ILeasingGoodsCalculatorService _service;

        public LeasingGoodsCalculatorController(ILeasingGoodsCalculatorService service)
        {
            _service = service;
        }

        [HttpPost("calculate")]
        public ActionResult<LeasingGoodsResponce>Calculate(
            [FromBody] LeasingGoodsRequest request)
        {
            return Ok(_service.Calculate(request));
        }
    }
}
