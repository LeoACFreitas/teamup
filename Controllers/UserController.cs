using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Teamup;

namespace Teamup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(MyDbContext db) : Controller
    {
        private readonly MyDbContext _db = db;

        [HttpGet]
        [Authorize]
        public IActionResult GetUser()
        {
            var user = HttpContext.GetCurrentUser(_db);

            if (user == null)
                return Content("null", "application/json");

            return Ok(user);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateUser([FromBody] User u)
        {
            u.Sub = Convert.ToDecimal(HttpContext.GetSubFromJwt());

            if (HttpContext.GetCurrentUser(_db) != null)
                return BadRequest("User already exists.");

            _db.User.Add(u);
            _db.SaveChanges();

            HttpContext.SendEmail("New user: " + JsonSerializer.Serialize(u));

            return Ok(true);
        }
    }
}
