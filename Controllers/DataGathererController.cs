using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Teamup.Data;

namespace Teamup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataGathererController(
        MyDbContext db,
        ILogger<DataGathererController> logger) : ControllerBase
    {
        private readonly MyDbContext _db = db;
        private readonly ILogger<DataGathererController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                using var json = JsonDocument.Parse(body);

                if (json.RootElement.ValueKind != JsonValueKind.Object)
                {
                    _logger.LogWarning(
                        "DataGatherer rejected JSON because its root is {JsonKind}. Body length: {BodyLength}",
                        json.RootElement.ValueKind,
                        body.Length);
                    return BadRequest();
                }

                if (!json.RootElement.TryGetProperty("password", out var password) ||
                    password.ValueKind != JsonValueKind.String ||
                    password.GetString() != "placeholder789.")
                {
                    _logger.LogWarning(
                        "DataGatherer rejected a request with a missing or invalid password. Body length: {BodyLength}",
                        body.Length);
                    return Unauthorized();
                }
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                    exception,
                    "DataGatherer rejected malformed JSON. Content-Type: {ContentType}; body length: {BodyLength}",
                    Request.ContentType,
                    body.Length);
                return BadRequest();
            }

            var data = new DataGatherer { Value = body };
            _db.DataGatherer.Add(data);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "DataGatherer stored entry {DataGathererId}. Body length: {BodyLength}",
                data.DataGatherer_id,
                body.Length);

            return Ok();
        }
    }
}
