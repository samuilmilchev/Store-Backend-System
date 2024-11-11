using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using Business.Services;
using CloudinaryDotNet.Actions;
using DAL.Data;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using WebApp1.Models;

namespace WebApp1.Tests.ControllerTests
{
    public class GamesServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly GameService _gameService;
        private readonly IMapper _mapper;
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IImagesService> _mockImagesService;

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

            _mockGameRepository = new Mock<IGameRepository>();
            _mockImagesService = new Mock<IImagesService>();

            _gameService = new GameService(_mockGameRepository.Object, _mapper, _mockImagesService.Object);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsTopPlatformsInDescendingOrder()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Game1", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M, Genre = "Game" },
                new Product { Id = 2, Name = "Game2", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M, Genre = "Game" },
                new Product { Id = 3, Name = "Game3", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 3.5, Price = 39.99M, Genre = "Game" },
                new Product { Id = 4, Name = "Game4", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 4.8, Price = 59.99M, Genre = "Game" },
                new Product { Id = 5, Name = "Game5", Platform = Platforms.Linux, DateCreated = DateTime.UtcNow, TotalRating = 3.0, Price = 49.99M, Genre = "Game" },
                new Product { Id = 6, Name = "Game6", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 5.0, Price = 25.00M, Genre = "Game" }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _mockGameRepository.Setup(repo => repo.GetTopPlatformsAsync())
                .ReturnsAsync(
                    new List<TopPlatformDto>
                    {
                new TopPlatformDto { Platform = Platforms.Windows.ToString(), ProductCount = 3 },
                new TopPlatformDto { Platform = Platforms.Mac.ToString(), ProductCount = 2 },
                new TopPlatformDto { Platform = Platforms.Linux.ToString(), ProductCount = 1 }
                    });

            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(Platforms.Windows.ToString(), result[0].Platform);
            Assert.Equal(3, result[0].ProductCount);
            Assert.Equal(Platforms.Mac.ToString(), result[1].Platform);
            Assert.Equal(2, result[1].ProductCount);
            Assert.Equal(Platforms.Linux.ToString(), result[2].Platform);
            Assert.Equal(1, result[2].ProductCount);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsEmptyList_WhenNoProductsExist()
        {
            // Arrange
            _mockGameRepository
                .Setup(repo => repo.GetTopPlatformsAsync())
                .ReturnsAsync(new List<TopPlatformDto>());

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
                new Product { Id = 1, Name = "Adventure Game", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M, Genre = "Adventure" },
                new Product { Id = 2, Name = "Action Game", Platform = Platforms.Windows, DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M, Genre = "Action" },
                new Product { Id = 3, Name = "Puzzle Game", Platform = Platforms.Mac, DateCreated = DateTime.UtcNow, TotalRating = 3.5, Price = 39.99M, Genre = "Puzzle" }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

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

            var mapper = mockMapper.Object;

            _mockGameRepository.Setup(repo => repo.SearchGamesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(products.Select(p => new SearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Platform = p.Platform.ToString(),
                    DateCreated = p.DateCreated,
                    TotalRating = p.TotalRating,
                    Price = p.Price
                }).ToList());

            var gameService = new GameService(_mockGameRepository.Object, mapper, _mockImagesService.Object);

            // Act
            var result = await gameService.SearchGamesAsync("Game", 10, 0);

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

        [Fact]
        public async Task SearchGameByIdAsync_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new SearchResultDto { Id = productId, Name = "Existing Product" };

            _mockGameRepository
                .Setup(repo => repo.SearchGameByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _gameService.SearchGameByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Equal(expectedProduct.Name, result.Name);
        }

        [Fact]
        public async Task SearchGameByIdAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;

            _mockGameRepository
                .Setup(repo => repo.SearchGameByIdAsync(productId))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Product not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.SearchGameByIdAsync(productId));
            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
        }

        [Fact]
        public async Task CreateProduct_CreatesProductWithImages_WhenImagesProvided()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "New Product",
                Logo = new Mock<IFormFile>().Object,
                Background = new Mock<IFormFile>().Object
            };

            var logoImage = new ImageUploadResult
            {
                Url = new Uri("http://example.com/logo.jpg")
            };

            var backgroundImage = new ImageUploadResult
            {
                Url = new Uri("http://example.com/background.jpg")
            };

            var expectedProduct = new SearchResultDto
            {
                Id = 1,
                Name = "New Product",
                Logo = logoImage.Url.ToString(),
                Background = backgroundImage.Url.ToString()
            };

            _mockImagesService.Setup(service => service.UploadImageAsync(createProductDto.Logo)).ReturnsAsync(logoImage);
            _mockImagesService.Setup(service => service.UploadImageAsync(createProductDto.Background)).ReturnsAsync(backgroundImage);

            _mockGameRepository.Setup(repo => repo.CreateGame(createProductDto)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _gameService.CreateGame(createProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Equal(logoImage.Url.ToString(), result.Logo);
            Assert.Equal(backgroundImage.Url.ToString(), result.Background);
        }

        [Fact]
        public async Task CreateProduct_CreatesProductWithoutImages_WhenNoImagesProvided()
        {
            // Arrange
            var createProductDto = new CreateProductDto { Name = "New Product" };
            var expectedProduct = new SearchResultDto
            {
                Id = 1,
                Name = "New Product",
                Logo = null,
                Background = null
            };

            _mockGameRepository.Setup(repo => repo.CreateGame(createProductDto)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _gameService.CreateGame(createProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Null(result.Logo);
            Assert.Null(result.Background);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProductWithImages_WhenImagesProvided()
        {
            // Arrange
            var productId = 1;
            var updateProductDto = new UpdateProductDto { Logo = new Mock<IFormFile>().Object, Background = new Mock<IFormFile>().Object };

            var logoImage = new ImageUploadResult { Url = new Uri("http://example.com/logo.jpg") };
            var backgroundImage = new ImageUploadResult { Url = new Uri("http://example.com/background.jpg") };

            var expectedProduct = new SearchResultDto
            {
                Id = productId,
                Name = "Updated Product",
                Logo = logoImage.Url.ToString(),
                Background = backgroundImage.Url.ToString()
            };

            _mockImagesService.Setup(service => service.UploadImageAsync(updateProductDto.Logo)).ReturnsAsync(logoImage);
            _mockImagesService.Setup(service => service.UploadImageAsync(updateProductDto.Background)).ReturnsAsync(backgroundImage);

            _mockGameRepository.Setup(repo => repo.UpdateGame(productId, updateProductDto)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _gameService.UpdateGame(productId, updateProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Equal(logoImage.Url.ToString(), result.Logo);
            Assert.Equal(backgroundImage.Url.ToString(), result.Background);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProductWithoutImages_WhenNoImagesProvided()
        {
            // Arrange
            var productId = 1;
            var updateProductDto = new UpdateProductDto { Name = "Updated Product" };

            var expectedProduct = new SearchResultDto
            {
                Id = productId,
                Name = "Updated Product",
                Logo = null,
                Background = null
            };

            _mockGameRepository.Setup(repo => repo.UpdateGame(productId, updateProductDto)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _gameService.UpdateGame(productId, updateProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Null(result.Logo);
            Assert.Null(result.Background);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsTrue_WhenProductIsDeleted()
        {
            // Arrange
            var productId = 1;
            _mockGameRepository.Setup(repo => repo.DeleteGame(productId)).ReturnsAsync(true);

            // Act
            var result = await _gameService.DeleteGame(productId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProduct_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            _mockGameRepository.Setup(repo => repo.DeleteGame(productId))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Product not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.DeleteGame(productId));
            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
        }
    }
}
