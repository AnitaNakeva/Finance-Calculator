using System.Security.Claims;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/calculations")]
    public class CalculationHistoryController : ControllerBase
    {
        private readonly IHistoryService _history;

        public CalculationHistoryController(IHistoryService history)
        {
            _history = history;
        }

        [HttpGet("history")]
        public async Task<ActionResult<CalculationHistoryResponse>> GetHistory(
            [FromQuery] string? calculationType,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 200) pageSize = 200;

            var response = await _history.GetHistoryAsync(userId, calculationType, from, to, search, sortOrder, page, pageSize);
            return Ok(response);
        }

        [HttpGet("history/export/csv")]
        public async Task<IActionResult> ExportCsv(
            [FromQuery] string? calculationType,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var csv = await _history.ExportCsvAsync(userId, calculationType, from, to, search);
            if (csv == null) return Unauthorized();
            return File(csv.Value.Content, csv.Value.ContentType, csv.Value.FileName);
        }

        [HttpGet("history/{id:int}")]
        public async Task<ActionResult<CalculationHistoryView>> GetHistoryItem(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var view = await _history.GetHistoryItemAsync(userId, id);
            if (view == null) return NotFound();
            return Ok(view);
        }

        [HttpPost("history/{id:int}/favorite")]
        public async Task<IActionResult> FavoriteFromHistory(int id, [FromBody] FavoriteFromHistoryRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var favorite = await _history.FavoriteFromHistoryAsync(userId, id, request?.Name);
            if (favorite == null) return NotFound();
            return CreatedAtAction(nameof(FavoritesController.Get), "Favorites", new { id = favorite.Id }, favorite);
        }
    }
}
