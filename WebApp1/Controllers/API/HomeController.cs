using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp1.Controllers.API
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getinfo")]
        public IActionResult GetInfo()
        {
            _logger.LogInformation("GetInfo endpoint was called.");

            return Ok(new { message = "Hello World" });
        }
    }
}
