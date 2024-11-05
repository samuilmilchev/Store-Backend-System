using AutoMapper;
using Business.Services;
using DAL.Data;
using DAL.Entities;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using WebApp1.Models;

namespace WebApp1.Tests.ControllerTests
{
    public class GamesServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly GameService _gameService;
        private readonly IMapper _mapper;
        private readonly GameRepository _gameRepository;

        public GamesServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<SearchResultDto>>(It.IsAny<List<Product>>()))
                .Returns((List<Product> products) => products.Select(p => new SearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Platform = p.Platform.ToString(),
                    DateCreated = p.DateCreated,
                    TotalRating = p.TotalRating,
                    Price = p.Price
                }).ToList());
            _mapper = mockMapper.Object;


            _gameRepository = new GameRepository(_context, _mapper);

            _gameService = new GameService(_gameRepository, _mapper);
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
        public async Task SearchGamesAsync_ThrowsValidationException_WhenTermIsEmpty()
        {
            // Arrange
            var emptySearchModel = new GameSearchModel { Term = null, Limit = 10, Offset = 0 };

            // Act & Assert
            var validationContext = new ValidationContext(emptySearchModel);
            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(emptySearchModel, validationContext, validateAllProperties: true));

            Assert.Equal("Search term is required.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsValidationException_WhenLimitIsNegative()
        {
            // Arrange
            var invalidLimitSearchModel = new GameSearchModel { Term = "Game", Limit = -1, Offset = 0 };

            // Act & Assert
            var validationContext = new ValidationContext(invalidLimitSearchModel);
            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(invalidLimitSearchModel, validationContext, validateAllProperties: true));

            Assert.Equal("Limit must be a non-negative value.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsValidationException_WhenOffsetIsNegative()
        {
            // Arrange
            var invalidOffsetSearchModel = new GameSearchModel { Term = "Game", Limit = 10, Offset = -1 };

            // Act & Assert
            var validationContext = new ValidationContext(invalidOffsetSearchModel);
            var exception = Assert.Throws<ValidationException>(() => Validator.ValidateObject(invalidOffsetSearchModel, validationContext, validateAllProperties: true));

            Assert.Equal("Offset must be a non-negative value.", exception.Message);
        }
    }
}
