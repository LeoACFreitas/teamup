using Microsoft.AspNetCore.Mvc;

namespace Teamup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController(MyDbContext db) : Controller
    {
        private readonly MyDbContext _db = db;

        [HttpGet]
        public IActionResult GetGame([FromQuery] string name = "")
        {
            name = name.ToUpper();

            if (string.IsNullOrEmpty(name))
            {
                return Ok(_db.Game.OrderByDescending(g => g.Value).Take(10).ToList());
            }

            var r = _db.Game.OrderByDescending(g => g.Value).Where(g => g.Name.ToUpper().StartsWith(name)).Take(5).Union(
                        _db.Game.OrderByDescending(g => g.Value).Where(g => g.Name.ToUpper().Contains(name)).Take(5)).ToList();
            return Ok(r);
        }
    }
}
