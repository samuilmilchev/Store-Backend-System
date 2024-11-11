using AutoMapper;
using Business.Exceptions;
using Business.Mappings;
using DAL.Data;
using DAL.Entities;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Enums;

namespace WebApp1.Tests.Repository_Tests
{
    public class GamesRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly GameRepository _gameRepository;
        private readonly IMapper _mapper;

        public GamesRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Products.AddRange(new List<Product>
            {
                new Product { Id = 1, Name = "Adventure Quest", Platform = Platforms.Windows, Genre = "Action" },
                new Product { Id = 2, Name = "Action Heroes", Platform = Platforms.Windows, Genre = "Adventure" },
                new Product { Id = 3, Name = "Puzzle Solver", Platform = Platforms.Mac, Genre = "Puzzle" }
            });
                context.SaveChanges();
            }

            var contextForRepo = new ApplicationDbContext(_options);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>());
            _mapper = config.CreateMapper();

            _gameRepository = new GameRepository(contextForRepo, _mapper);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsTopPlatformsByProductCount()
        {
            // Act
            var result = await _gameRepository.GetTopPlatformsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Windows", result[0].Platform);
            Assert.Equal(2, result[0].ProductCount);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsEmptyList_WhenNoProductsInDatabase()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.Products.RemoveRange(context.Products);
                context.SaveChanges();
            }

            // Act
            var result = await _gameRepository.GetTopPlatformsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_CorrectlyCountsMultiplePlatforms()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.Products.AddRange(new List<Product>
                {
                    new Product { Id = 4, Name = "Mystery Puzzle", Platform = Platforms.Mac, Genre = "Puzzle" },
                    new Product { Id = 5, Name = "Puzzle Adventure", Platform = Platforms.Windows, Genre = "Puzzle" }
                });
                context.SaveChanges();
            }

            // Act
            var result = await _gameRepository.GetTopPlatformsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Windows", result[0].Platform);
            Assert.Equal(3, result[0].ProductCount);
            Assert.Equal("Mac", result[1].Platform);
            Assert.Equal(2, result[1].ProductCount);
        }

        [Fact]
        public async Task SearchGamesAsync_ReturnsMatchingGamesByTerm()
        {
            // Act
            var result = await _gameRepository.SearchGamesAsync("Adventure", 10, 0);

            // Assert
            Assert.Single(result);
            Assert.Equal("Adventure Quest", result[0].Name);
        }

        [Fact]
        public async Task SearchGamesAsync_ReturnsEmptyList_WhenNoMatchingGames()
        {
            // Act
            var result = await _gameRepository.SearchGamesAsync("Nonexistent", 10, 0);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchGamesAsync_LimitsResultsCorrectly()
        {
            // Act
            var result = await _gameRepository.SearchGamesAsync("Quest", 1, 0);

            // Assert
            Assert.Single(result);
            Assert.Equal("Adventure Quest", result[0].Name);
        }

        [Fact]
        public async Task SearchGamesAsync_ThrowsNotFoundException_WhenTermIsEmpty()
        {
            // Act
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameRepository.SearchGamesAsync("", 10, 0));
            // Assert
            Assert.Equal("Invalid input.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_ReturnsEmptyList_WhenOffsetExceedsResults()
        {
            // Act
            var result = await _gameRepository.SearchGamesAsync("Action", 10, 5);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchGameByIdAsync_ReturnsCorrectGame_WhenIdExists()
        {
            // Act
            var result = await _gameRepository.SearchGameByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Adventure Quest", result.Name);
            Assert.Equal("Action", result.Genre);
        }

        [Fact]
        public async Task SearchGameByIdAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameRepository.SearchGameByIdAsync(999));
            Assert.Equal("Invalid input.", exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_RespectsLimitAndOffset()
        {
            // Act
            var result = await _gameRepository.SearchGamesAsync("Action", 1, 0);

            // Assert
            Assert.Single(result);
            Assert.Equal("Action Heroes", result[0].Name);
        }

        [Fact]
        public async Task CreateProduct_AddsNewProductToDatabase()
        {
            // Arrange
            var newProduct = new CreateProductDto()
            {
                Name = "New Game",
                Platform = Platforms.Windows,
                Genre = "RPG"
            };

            // Act
            await _gameRepository.CreateProduct(newProduct);

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var productInDb = await context.Products.FindAsync(4);
                Assert.NotNull(productInDb);
                Assert.Equal("New Game", productInDb.Name);
                Assert.Equal(Platforms.Windows, productInDb.Platform);
                Assert.Equal("RPG", productInDb.Genre);
            }
        }

        [Fact]
        public async Task UpdateProduct_UpdatesExistingProductFields()
        {
            // Arrange
            var productId = 1;
            var updatedProduct = new UpdateProductDto
            {
                Name = "Updated Game Name",
                Platform = Platforms.Mac,
                Genre = "Strategy"
            };

            // Act
            await _gameRepository.UpdateProduct(productId, updatedProduct);

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var productInDb = await context.Products.FindAsync(productId);
                Assert.NotNull(productInDb);
                Assert.Equal("Updated Game Name", productInDb.Name);
                Assert.Equal(Platforms.Mac, productInDb.Platform);
                Assert.Equal("Strategy", productInDb.Genre);
            }
        }

        [Fact]
        public async Task UpdateProduct_ThrowExceptionNotFound()
        {
            // Arrange
            var productId = 5;
            var updatedProduct = new UpdateProductDto
            {
                Name = "Updated Game Name",
                Platform = Platforms.Mac,
                Genre = "Strategy"
            };

            // Act
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameRepository.UpdateProduct(productId, updatedProduct));

            // Assert
            Assert.Equal($"Product with id {productId} does not exist.", exception.Message);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProductFromDatabase()
        {
            // Arrange
            var productId = 1;

            // Act
            await _gameRepository.DeleteProduct(productId);

            // Assert
            using (var context = new ApplicationDbContext(_options))
            {
                var productInDb = await context.Products.FindAsync(productId);

                Assert.True(productInDb.IsDeleted);
            }
        }

        [Fact]
        public async Task DeleteProduct_ThrowExceptionNotFound()
        {
            // Arrange
            var productId = 5;

            // Act
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameRepository.DeleteProduct(productId));

            // Assert
            Assert.Equal($"Product with id {productId} was not found.", exception.Message);

        }
    }
}
