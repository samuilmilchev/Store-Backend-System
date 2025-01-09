using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using Business.Mappings;
using Business.Services;
using CloudinaryDotNet.Actions;
using DAL.Data;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
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

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RatingProfile>();
                cfg.AddProfile<ProductProfile>();
            });
            _mapper = config.CreateMapper();

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

            _mockGameRepository.Setup(repo => repo.SearchGamesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(products);

            // Act
            var result = await _gameService.SearchGamesAsync("Game", 10, 0);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.Name == "Adventure Game");
            Assert.Contains(result, r => r.Name == "Action Game");
            Assert.Contains(result, r => r.Name == "Puzzle Game");
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
            var product = new Product { Id = 1, Name = "Existing Product" };
            var allProducts = new List<Product> { product };

            _mockGameRepository.Setup(repo => repo.GetProducts())
                .ReturnsAsync(allProducts);

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(product);

            // Act
            var result = await _gameService.SearchGameByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task SearchGameByIdAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            var allProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Existing Product" }
            };

            _mockGameRepository.Setup(repo => repo.GetProducts())
                .ReturnsAsync(allProducts);

            _mockGameRepository
                .Setup(repo => repo.SearchGameByIdAsync(productId))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.InvalidData, "Invalid input."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.SearchGameByIdAsync(productId));
            Assert.Equal(ErrorStatus.InvalidData, exception.ErrorStatus);
        }

        [Fact]
        public async Task CreateGame_CreatesProductWithImages_WhenImagesProvided()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "New Product",
                Logo = new Mock<IFormFile>().Object,
                Background = new Mock<IFormFile>().Object
            };

            var logoImage = new ImageUploadResult { Url = new Uri("http://example.com/logo.jpg") };
            var backgroundImage = new ImageUploadResult { Url = new Uri("http://example.com/background.jpg") };

            var product = new Product { Id = 1, Name = "New Product", Logo = logoImage.Url.ToString(), Background = backgroundImage.Url.ToString() };

            _mockImagesService.Setup(service => service.UploadImageAsync(createProductDto.Logo)).ReturnsAsync(logoImage);
            _mockImagesService.Setup(service => service.UploadImageAsync(createProductDto.Background)).ReturnsAsync(backgroundImage);

            _mockGameRepository.Setup(repo => repo.CreateGame(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _gameService.CreateGame(createProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(logoImage.Url.ToString(), result.Logo);
            Assert.Equal(backgroundImage.Url.ToString(), result.Background);
        }

        [Fact]
        public async Task CreateProduct_CreatesProductWithoutImages_WhenNoImagesProvided()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "New Product",
            };

            var product = new Product { Id = 1, Name = "New Product" };

            _mockGameRepository.Setup(repo => repo.CreateGame(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _gameService.CreateGame(createProductDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateGame_UpdatesProductWithImages_WhenImagesProvided()
        {
            // Arrange
            var productId = 1;
            var updateProductDto = new UpdateProductDto
            {
                Logo = new Mock<IFormFile>().Object,
                Background = new Mock<IFormFile>().Object
            };

            var logoImage = new ImageUploadResult { Url = new Uri("http://example.com/logo.jpg") };
            var backgroundImage = new ImageUploadResult { Url = new Uri("http://example.com/background.jpg") };

            var product = new Product { Id = productId, Name = "Updated Product" };

            _mockImagesService.Setup(service => service.UploadImageAsync(updateProductDto.Logo)).ReturnsAsync(logoImage);
            _mockImagesService.Setup(service => service.UploadImageAsync(updateProductDto.Background)).ReturnsAsync(backgroundImage);

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(productId)).ReturnsAsync(product);
            _mockGameRepository.Setup(repo => repo.UpdateGame(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _gameService.UpdateGame(productId, updateProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(logoImage.Url.ToString(), result.Logo);
            Assert.Equal(backgroundImage.Url.ToString(), result.Background);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProductWithoutImages_WhenNoImagesProvided()
        {
            // Arrange
            var productId = 1;
            var updateProductDto = new UpdateProductDto
            {
                Name = "Update"
            };

            var product = new Product { Id = productId, Name = "Updated Product" };

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(productId)).ReturnsAsync(product);
            _mockGameRepository.Setup(repo => repo.UpdateGame(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _gameService.UpdateGame(productId, updateProductDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
        }

        [Fact]
        public async Task DeleteGame_ReturnsTrue_WhenProductIsDeleted()
        {
            // Arrange
            var product = new Product { Id = 1 };

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
            _mockGameRepository.Setup(repo => repo.DeleteGame(product)).ReturnsAsync(true);

            // Act
            var result = await _gameService.DeleteGame(product.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProduct_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var product = new Product { Id = 1 };

            _mockGameRepository.Setup(repo => repo.DeleteGame(product)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.DeleteGame(product.Id));
            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
        }

        [Fact]
        public async Task CreateRating_ReturnsRatingResponse_WhenDataIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingData = new CreateRatingDto { ProductId = 1, Rating = 4 };

            ProductRating rating = _mapper.Map<ProductRating>(ratingData);

            Assert.NotNull(rating);

            rating.UserId = userId;

            var product = new Product { Id = ratingData.ProductId, Ratings = new List<ProductRating>() };

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(ratingData.ProductId))
                               .ReturnsAsync(product);

            _mockGameRepository.Setup(repo => repo.CreateRating(It.IsAny<Product>(), It.IsAny<ProductRating>()))
                               .ReturnsAsync(rating);

            // Act
            var result = await _gameService.CreateRating(userId, ratingData);

            // Assert
            Assert.Equal(ratingData.ProductId, result.ProductId);
            Assert.Equal(ratingData.Rating, result.Rating);
        }

        [Fact]
        public async Task CreateRating_ThrowsMyApplicationException_WhenProductDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingData = new CreateRatingDto { ProductId = 999, Rating = 4 };

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(ratingData.ProductId))
                               .ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.CreateRating(userId, ratingData));
            Assert.Equal("Product with id 999 was not found.", exception.Message);
        }

        [Fact]
        public async Task CreateRating_ThrowsInvalidOperationException_WhenUserHasAlreadyRated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ratingData = new CreateRatingDto { ProductId = 1, Rating = 4 };

            var existingRating = new ProductRating
            {
                ProductId = ratingData.ProductId,
                UserId = userId,
                Rating = 5
            };

            var product = new Product
            {
                Id = ratingData.ProductId,
                Ratings = new List<ProductRating> { existingRating }
            };

            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(ratingData.ProductId))
                               .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() =>
                _gameService.CreateRating(userId, ratingData));

            Assert.Equal(ErrorStatus.InvalidOperation, exception.ErrorStatus);
            Assert.Equal("You have already review this product.", exception.Message);
        }

        [Fact]
        public async Task DeleteRating_CompletesSuccessfully_WhenRatingExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var deleteRatingData = new DeleteRatingDto { ProductId = 1 };
            var product = new Product { Id = deleteRatingData.ProductId, Ratings = new List<ProductRating>() };
            var rating = new ProductRating { ProductId = deleteRatingData.ProductId, UserId = userId, Rating = 5 };

            _mockGameRepository.Setup(repo => repo.GetProducts())
                               .ReturnsAsync(new List<Product> { product });
            _mockGameRepository.Setup(repo => repo.GetRatings())
                               .Returns(new List<ProductRating> { rating });

            // Act & Assert
            await _gameService.DeleteRating(userId, deleteRatingData);
        }

        [Fact]
        public async Task DeleteRating_ThrowsMyApplicationException_WhenRatingDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var deleteRatingData = new DeleteRatingDto { ProductId = 999 };

            _mockGameRepository.Setup(repo => repo.GetProducts())
                               .ReturnsAsync(new List<Product>());
            _mockGameRepository.Setup(repo => repo.GetRatings())
                               .Returns(new List<ProductRating>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _gameService.DeleteRating(userId, deleteRatingData));
            Assert.Equal("Rating was not found.", exception.Message);
        }

        [Fact]
        public async Task ListGames_ReturnsListOfProducts_WhenQueryIsValid()
        {
            // Arrange
            var queryData = new ProductQueryDto { Genre = "Action", SortBy = SortBy.Price, SortDirection = SortDirection.Asc, PageSize = 2 };

            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Game1", Genre = "Action", Price = 20 },
                new Product { Id = 2, Name = "Game2", Genre = "Action", Price = 25 }
            };

            var expectedResult = new ProductListResultDto
            {
                Products = products.Select(p => new SearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Genre = p.Genre,
                    Price = p.Price
                }).ToList(),
                TotalItems = 2,
                Page = 1,
                PageSize = 2
            };

            _mockGameRepository.Setup(repo => repo.ListGames(queryData))
                               .ReturnsAsync(products);

            // Act
            var result = await _gameService.ListGames(queryData);

            // Assert
            Assert.Equal(expectedResult.Products.Count(), result.Products.Count());
            Assert.Equal(expectedResult.TotalItems, result.TotalItems);
            Assert.Equal(expectedResult.PageSize, result.PageSize);
            Assert.Equal(expectedResult.Page, result.Page);
        }

        [Fact]
        public async Task ListGames_ReturnsEmptyList_WhenNoGamesMatchQuery()
        {
            // Arrange
            var queryData = new ProductQueryDto { Genre = "Nonexistent Genre" };

            _mockGameRepository.Setup(repo => repo.ListGames(queryData))
                               .ReturnsAsync(new List<Product>());

            // Act
            var result = await _gameService.ListGames(queryData);

            // Assert
            Assert.Empty(result.Products);
            Assert.Equal(0, result.TotalItems);
        }

        [Fact]
        public async Task ListGames_ReturnsPaginatedResults_WhenPageAndPageSizeAreSpecified()
        {
            // Arrange
            var queryData = new ProductQueryDto { Page = 1, PageSize = 2 };
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Game1" },
                new Product { Id = 2, Name = "Game2" },
                new Product { Id = 3, Name = "Game3" }
            };

            var paginatedResult = products.Skip((queryData.Page - 1) * queryData.PageSize)
                                           .Take(queryData.PageSize)
                                           .ToList();

            var expectedResult = new ProductListResultDto
            {
                Products = paginatedResult.Select(p => new SearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList(),
                TotalItems = products.Count,
                Page = queryData.Page,
                PageSize = queryData.PageSize
            };

            _mockGameRepository.Setup(repo => repo.ListGames(queryData))
                               .ReturnsAsync(products);

            // Act
            var result = await _gameService.ListGames(queryData);

            // Assert
            Assert.Equal(products.Count, result.TotalItems);
            Assert.Equal(queryData.Page, result.Page);
            Assert.Equal(queryData.PageSize, result.PageSize);
        }

        [Fact]
        public async Task ListGames_ThrowsArgumentException_WhenSortingParametersAreInvalid()
        {
            // Arrange
            var invalidQueryData = new ProductQueryDto { SortBy = (SortBy)99, SortDirection = (SortDirection)99 };

            _mockGameRepository.Setup(repo => repo.ListGames(invalidQueryData))
                               .ThrowsAsync(new ArgumentException("Invalid sorting parameters"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _gameService.ListGames(invalidQueryData));
        }
    }
}
