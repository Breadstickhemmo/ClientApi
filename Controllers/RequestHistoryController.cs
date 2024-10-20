using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiApp.Data;

namespace MyApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestHistoryController : ControllerBase
    {
        private readonly HistoryDbContext _historyDbContext;

        public RequestHistoryController(HistoryDbContext historyDbContext)
        {
            _historyDbContext = historyDbContext;
        }

        // GET: api/RequestHistory
        [HttpGet]
        public async Task<IActionResult> GetRequestHistory()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                var History = await _historyDbContext.History
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                return Ok(History);
            }

            return Unauthorized();
        }

        // DELETE: api/RequestHistory
        [HttpDelete]
        public async Task<IActionResult> DeleteRequestHistory()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                var userHistory = _historyDbContext.History
                    .Where(r => r.UserId == userId);

                _historyDbContext.History.RemoveRange(userHistory);
                await _historyDbContext.SaveChangesAsync();

                return NoContent();
            }

            return Unauthorized();
        }
    }
}