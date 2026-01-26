using System.Security.Claims;
using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/calculations/favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesService _favorites;

        public FavoritesController(IFavoritesService favorites)
        {
            _favorites = favorites;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteCalculation>>> List()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var items = await _favorites.ListAsync(userId);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FavoriteCalculation>> Get(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var favorite = await _favorites.GetAsync(userId, id);
            if (favorite == null) return NotFound();

            return Ok(favorite);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Rename(int id, [FromBody] RenameFavoriteRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name is required.");

            var result = await _favorites.RenameAsync(userId, id, request.Name.Trim());
            if (!result.success)
            {
                if (!string.IsNullOrEmpty(result.conflict)) return Conflict(result.conflict);
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var ok = await _favorites.DeleteAsync(userId, id);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
