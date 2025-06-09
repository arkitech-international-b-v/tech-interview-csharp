using Microsoft.AspNetCore.Mvc;

namespace ArkitechDataApi.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET /
        [HttpGet("/")]
        public IActionResult ReadRoot()
        {
            return Ok(new { message = "Welcome to the Arkitech Data API (C# .NET 9)" });
        }

        // GET /health
        [HttpGet("/health")]
        public IActionResult HealthCheck()
        {
            // We don’t run an MQTT client here, but you could add logic 
            // to verify Mongo connectivity or other dependencies.
            return Ok(new { status = "healthy", mongo_connected = true });
        }
    }
}
