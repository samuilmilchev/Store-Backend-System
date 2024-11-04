using Business.Exceptions;
using Business.Services;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApp1.Tests.ControllerTests
{
    public class GamesServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly GameService _gameService;

        public GamesServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name
             .Options;

            _context = new ApplicationDbContext(options);
            _gameService = new GameService(_context);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsTopPlatformsInDescendingOrder()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Game1", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M },
                new Product { Id = 2, Name = "Game2", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M },
                new Product { Id = 3, Name = "Game3", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 3.5, Price = 39.99M },
                new Product { Id = 4, Name = "Game4", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 4.8, Price = 59.99M },
                new Product { Id = 5, Name = "Game5", Platform = Platforms.Linux, DateCreated = DateTime.UtcNow, TotalRating = 3.0, Price = 49.99M },
                new Product { Id = 6, Name = "Game6", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 5.0, Price = 25.00M }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(Platforms.Windows.ToString(), result[0].Platform);
            Assert.Equal(3, result[0].ProductCount); // Should be 4 for Windows
            Assert.Equal(Platforms.Mac.ToString(), result[1].Platform);
            Assert.Equal(2, result[1].ProductCount); // Should be 2 for Mac
            Assert.Equal(Platforms.Linux.ToString(), result[2].Platform);
            Assert.Equal(1, result[2].ProductCount); // Should be 1 for Linux
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsEmptyList_WhenNoProductsExist()
        {
            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchGamesAsync_ReturnsMatchingGames_WhenTermIsValid()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Adventure Game", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M },
                new Product { Id = 2, Name = "Action Game", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M },
                new Product { Id = 3, Name = "Puzzle Game", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 3.5, Price = 39.99M }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.SearchGamesAsync("Game", 10, 0);

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsException_WhenTermIsEmpty()
        {
            // Arrange
            string emptyTerm = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                () => _gameService.SearchGamesAsync(emptyTerm, 10, 0)
            );
            Assert.Equal("Invalid search term.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsException_WhenLimitIsNegative()
        {
            // Arrange
            string validTerm = "Game";
            int negativeLimit = -1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                () => _gameService.SearchGamesAsync(validTerm, negativeLimit, 0)
            );
            Assert.Equal("Invalid search term.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsException_WhenOffsetIsNegative()
        {
            // Arrange
            string validTerm = "Game";
            int negativeOffset = -1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(
                () => _gameService.SearchGamesAsync(validTerm, 10, negativeOffset)
            );
            Assert.Equal("Invalid search term.", exception.Message);
        }
    }
}
