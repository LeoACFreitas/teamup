using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Teamup.Data;

namespace Teamup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataGathererController(MyDbContext db) : ControllerBase
    {
        private readonly MyDbContext _db = db;

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                using var json = JsonDocument.Parse(body);

                if (!json.RootElement.TryGetProperty("password", out var password) ||
                    password.ValueKind != JsonValueKind.String ||
                    password.GetString() != "placeholder789.")
                    return Unauthorized();
            }
            catch (JsonException)
            {
                return BadRequest();
            }

            _db.DataGatherer.Add(new DataGatherer { Value = body });
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
