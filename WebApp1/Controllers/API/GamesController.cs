using Business.Intefraces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using WebApp1.Models;

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
        public async Task<IActionResult> SearchGames([FromQuery] GameSearchModel searchModel)
        {
            var searchResults = await _gameService.SearchGamesAsync(searchModel.Term, searchModel.Limit, searchModel.Offset);

            return Ok(searchResults);
        }

        /// <summary>
        /// Retrieves the details of a game by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the game to search for.</param>
        /// <returns>Returns the game details matching the provided ID.</returns>
        /// <response code="200">Returns the game details if the game is found.</response>
        /// <response code="404">Not found if no game exists with the specified ID.</response>
        [HttpGet("id")]
        public async Task<IActionResult> SearchGameById([FromQuery] int id)
        {
            var searchResults = await _gameService.SearchGameByIdAsync(id);

            return Ok(searchResults);
        }

        /// <summary>
        /// Creates a new game with the provided product data.
        /// </summary>
        /// <param name="productData">The data for the product to be created, including name, platform, price, genre and images.</param>
        /// <returns>Returns the created product with a 201 status code.</returns>
        /// <response code="201">Returns the newly created product.</response>
        /// <response code="400">Bad request if the provided data is invalid.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromForm] CreateProductDto productData)
        {
            var createResult = await _gameService.CreateGame(productData);

            return CreatedAtAction(nameof(SearchGameById), new { id = createResult.Id }, createResult);
        }

        /// <summary>
        /// Updates an existing game with the provided product data.
        /// </summary>
        /// <param name="id">The unique identifier of the product to be updated.</param>
        /// <param name="updatedData">The updated data for the product, including optional fields like logo and background.</param>
        /// <returns>Returns the updated product with a 200 status code.</returns>
        /// <response code="200">Returns the updated product data.</response>
        /// <response code="400">Bad request if the provided data is invalid.</response>
        /// <response code="404">Not found if the product with the specified ID does not exist.</response>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromForm] UpdateProductDto updatedData)
        {
            var createResult = await _gameService.UpdateGame(id, updatedData);

            return Ok(createResult);
        }

        /// <summary>
        /// Deletes the game with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the product to be deleted.</param>
        /// <returns>Returns a 204 status code if the product is successfully deleted.</returns>
        /// <response code="204">Product successfully deleted.</response>
        /// <response code="404">Not Found if the product with the specified ID does not exist.</response>
        /// <response code="500">Internal server error if the deletion fails.</response>
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var isDeleted = await _gameService.DeleteGame(id);

            return NoContent();
        }
    }
}
