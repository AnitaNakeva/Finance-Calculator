using FinanceCalculator.API.Contracts;
using FinanceCalculator.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceCalculator.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;

        public AdminController(IAdminService admin)
        {
            _admin = admin;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _admin.GetUsersAsync();
            return Ok(users);
        }

        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
        {
            var result = await _admin.ChangeRoleAsync(id, request.Role?.Trim() ?? string.Empty);
            if (!result.success) return string.IsNullOrEmpty(result.error) ? NotFound() : BadRequest(result.error);
            return NoContent();
        }

        [HttpGet("calculations")]
        public async Task<ActionResult<IEnumerable<object>>> GetCalculations([FromQuery] int? userId)
        {
            var items = await _admin.GetCalculationsAsync(userId);
            return Ok(items);
        }

        [HttpGet("audit")]
        public async Task<ActionResult<IEnumerable<object>>> GetAudit([FromQuery] int? userId)
        {
            var items = await _admin.GetAuditAsync(userId);
            return Ok(items);
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> Cleanup([FromQuery] int days = 30)
        {
            await _admin.CleanupAsync(days);
            return NoContent();
        }
    }
}
