using Business.Intefraces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp1.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Retrieves the top three popular platforms based on the number of products.
        /// </summary>
        /// <returns>Returns a list of top platforms.</returns>
        /// <response code="200">Returns a list of the top platforms.</response>
        [HttpGet("topPlatforms")]
        public async Task<IActionResult> GetTopPlatforms()
        {
            var topPlatforms = await _gameService.GetTopPlatformsAsync();

            return Ok(topPlatforms);
        }

        /// <summary>
        /// Searches for games that match the specified search term.
        /// </summary>
        /// <param name="term">The search term to filter games.</param>
        /// <param name="limit">The maximum number of items to return (default is 10).</param>
        /// <param name="offset">The number of items to skip (default is 0).</param>
        /// <returns>Returns a list of games that match the search criteria.</returns>
        /// <response code="200">Returns a list of games matching the search term.</response>
        /// <response code="400">Bad request if the search term is invalid.</response>
        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string term, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var searchResults = await _gameService.SearchGamesAsync(term, limit, offset);

            return Ok(searchResults);
        }
    }
}
