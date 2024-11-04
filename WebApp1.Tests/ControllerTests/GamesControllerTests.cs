using Business.Intefraces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.DTOs;
using WebApp1.Controllers.API;

namespace WebApp1.Tests.ControllerTests
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            _mockGameService = new Mock<IGameService>();
            _controller = new GamesController(_mockGameService.Object);
        }

        [Fact]
        public async Task GetTopPlatforms_ReturnsOkResult_WithExpectedData()
        {
            // Arrange
            var platforms = new List<TopPlatformDto>
            {
                new TopPlatformDto { Platform = "Platform1", ProductCount = 100 },
                new TopPlatformDto { Platform = "Platform2", ProductCount = 80 },
                new TopPlatformDto { Platform = "Platform3", ProductCount = 60 }
            };

            _mockGameService.Setup(s => s.GetTopPlatformsAsync()).ReturnsAsync(platforms);

            // Act
            var result = await _controller.GetTopPlatforms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(platforms, okResult.Value);
        }

        [Fact]
        public async Task SearchGames_ReturnsEmptyList_WhenNoGamesFound()
        {
            // Arrange
            var searchTerm = "NonExistentGame";
            var games = new List<SearchResultDto>();

            _mockGameService.Setup(s => s.SearchGamesAsync(searchTerm, 10, 0)).ReturnsAsync(games);

            // Act
            var result = await _controller.SearchGames(searchTerm, 10, 0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Empty(okResult.Value as List<SearchResultDto>);
        }


        [Fact]
        public async Task SearchGames_ReturnsOkResult_WithMatchingGames()
        {
            // Arrange
            var searchTerm = "Game";
            var games = new List<SearchResultDto>
            {
                new SearchResultDto { Id = 1, Name = "Game1", Platform = "Platform1", DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M },
                new SearchResultDto { Id = 2, Name = "Game2", Platform = "Platform2", DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M }
            };

            _mockGameService.Setup(s => s.SearchGamesAsync(searchTerm, 10, 0)).ReturnsAsync(games);

            // Act
            var result = await _controller.SearchGames(searchTerm, 10, 0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(games, okResult.Value);
        }

        [Fact]
        public async Task SearchGames_ReturnsNull_WhenTermIsInvalid()
        {
            // Arrange
            string invalidTerm = null;

            // Act
            var result = await _controller.SearchGames(invalidTerm, 10, 0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

    }
}
