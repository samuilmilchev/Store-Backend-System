using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp1.Controllers.API
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {

        [HttpGet("getinfo")]
        public IActionResult GetInfo()
        {
            return Ok(new { message = "Hello World"});
        }
    }
}
