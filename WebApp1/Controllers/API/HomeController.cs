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

        [HttpGet("getinfo")]
        public IActionResult GetInfo()
        {
            _logger.LogInformation("GetInfo endpoint was called.");

            //throw new Exception("This is a simple exception for testing.");

            return Ok(new { message = "Hello World" });
        }
    }
}
