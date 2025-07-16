using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Teamup.Data;
using Teamup.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Teamup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestController(MyDbContext db) : ControllerBase
    {
        private readonly MyDbContext _db = db;

        [HttpGet]
        public async Task<IActionResult> GetRequests(
            [FromQuery] string? gameName,
            [FromQuery] bool? showMyRequests,
            [FromQuery] string? country,
            [FromQuery] int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _db.Request.AsQueryable();
            var pageSize = 20;

            if (!string.IsNullOrEmpty(gameName))
                query = query.Where(r => r.Game != null && r.Game.Name.Equals(gameName));

            if (showMyRequests.HasValue && showMyRequests.Value && userId != null)
                query = query.Where(r => r.User != null && r.User.Sub == Convert.ToDecimal(userId));

            if (!string.IsNullOrEmpty(country))
                query = query.Where(r => r.User != null && r.User.Country.Equals(country, StringComparison.OrdinalIgnoreCase));

            var requests = await query.OrderByDescending(r => r.Date)
                                      .Include(r => r.User)
                                      .Include(r => r.Game)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return Ok(requests);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateRequest([FromBody] Request request)
        {
            Game? game = null;
            if (request.Game != null)
                game = _db.Game.FirstOrDefault(g => g.Name.Equals(request.Game.Name));

            if (game == null)
                return BadRequest();

            if (_db.Request.Count(r => r.User.Sub == Convert.ToDecimal(HttpContext.GetSubFromJwt())) >= 2)
                return BadRequest("You have reached the maximum number of requests, delete any to continue");

            request.Date = DateTime.Now;
            request.Game = game;
            request.User = HttpContext.GetCurrentUser(_db);
            _db.Request.Add(request);

            HttpContext.SendEmail("New request added: " + request.User.Nickname);

            _db.SaveChanges();

            return Ok(request);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteRequest(int id)
        {
            var request = _db.Request.Include(r => r.User).FirstOrDefault(r => r.Request_id == id);

            if (request == null || request.User.Sub != Convert.ToDecimal(HttpContext.GetSubFromJwt()))
                return BadRequest();

            _db.Request.Remove(request);
            _db.SaveChanges();

            return NoContent();
        }
    }
}
