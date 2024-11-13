using Business.Exceptions;
using Business.Intefraces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.DTOs;
using System.Security.Claims;
using WebApp1.Controllers.API;
using WebApp1.Models;

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
            var searchModel = new GameSearchModel
            {
                Term = "NonExistentGame",
                Limit = 10,
                Offset = 0
            };
            var games = new List<SearchResultDto>();

            _mockGameService.Setup(s => s.SearchGamesAsync(searchModel.Term, searchModel.Limit, searchModel.Offset)).ReturnsAsync(games);



            // Act
            var result = await _controller.SearchGames(searchModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Empty(okResult.Value as List<SearchResultDto>);
        }

        [Fact]
        public async Task SearchGames_ReturnsOkResult_WithMatchingGames()
        {
            // Arrange
            var searchModel = new GameSearchModel
            {
                Term = "Game",
                Limit = 10,
                Offset = 0
            };

            var games = new List<SearchResultDto>
            {
                new SearchResultDto { Id = 1, Name = "Game1", Platform = "Platform1", DateCreated = DateTime.UtcNow, TotalRating = 4.5, Price = 19.99M },
                new SearchResultDto { Id = 2, Name = "Game2", Platform = "Platform2", DateCreated = DateTime.UtcNow, TotalRating = 4.0, Price = 29.99M }
            };

            _mockGameService.Setup(s => s.SearchGamesAsync(searchModel.Term, searchModel.Limit, searchModel.Offset)).ReturnsAsync(games);

            // Act
            var result = await _controller.SearchGames(searchModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(games, okResult.Value);
        }

        [Fact]
        public async Task SearchGames_ReturnsNull_WhenTermIsInvalid()
        {
            // Arrange
            var searchModel = new GameSearchModel
            {
                Term = null,
                Limit = 10,
                Offset = 0
            };
            GameSearchModel gameSearchModel = new GameSearchModel();

            // Act
            var result = await _controller.SearchGames(searchModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task SearchGameById_ReturnsOkResult_WhenGameExists()
        {
            // Arrange
            var gameId = 1;
            var game = new SearchResultDto { Id = gameId, Name = "Test Game" };

            _mockGameService.Setup(s => s.SearchGameByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _controller.SearchGameById(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(game, okResult.Value);
        }

        [Fact]
        public async Task SearchGameById_ThrowsMyApplicationException_ReturnsNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            _mockGameService
                .Setup(s => s.SearchGameByIdAsync(nonExistentId))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Game not found"));

            // Act
            var result = await Assert.ThrowsAsync<MyApplicationException>(() => _controller.SearchGameById(nonExistentId));

            // Assert
            Assert.Equal(ErrorStatus.NotFound, result.ErrorStatus);
            Assert.Equal("Game not found", result.Message);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreatedAtActionResult_WithCreatedProduct()
        {
            // Arrange
            var productData = new CreateProductDto { Name = "New Product", Price = 29.99M };
            var createdProduct = new SearchResultDto { Id = 1, Name = productData.Name, Price = productData.Price };

            _mockGameService.Setup(s => s.CreateGame(productData)).ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.CreateGame(productData);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.Equal(createdProduct, createdAtResult.Value);
            Assert.Equal(nameof(_controller.SearchGameById), createdAtResult.ActionName);
            Assert.Equal(createdProduct.Id, createdAtResult.RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsOkResult_WithUpdatedProduct()
        {
            // Arrange
            var productId = 1;
            var updatedData = new UpdateProductDto { Name = "Updated Product", Price = 39.99M };
            var updatedProduct = new SearchResultDto { Id = productId, Name = updatedData.Name, Price = (decimal)updatedData.Price };

            _mockGameService.Setup(s => s.UpdateGame(productId, updatedData)).ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateGame(productId, updatedData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(updatedProduct, okResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_ThrowsMyApplicationException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            var updatedData = new UpdateProductDto { Name = "Nonexistent Product", Price = 49.99M };

            _mockGameService
                .Setup(s => s.UpdateGame(productId, updatedData))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Product not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _controller.UpdateGame(productId, updatedData));

            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
            Assert.Equal("Product not found", exception.Message);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductDeletedSuccessfully()
        {
            // Arrange
            var productId = 1;

            _mockGameService.Setup(s => s.DeleteGame(productId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteGame(productId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ThrowsMyApplicationException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;

            _mockGameService
                .Setup(s => s.DeleteGame(productId))
                .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Product not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _controller.DeleteGame(productId));

            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
            Assert.Equal("Product not found", exception.Message);
        }

        [Fact]
        public async Task CreateRating_ReturnsOkResult_WithCreatedRating()
        {
            // Arrange
            var ratingData = new CreateRatingDto { ProductId = 1, Rating = 4 };
            var userId = Guid.NewGuid();
            var createdRating = new RatingResponseDto { ProductId = ratingData.ProductId, Rating = ratingData.Rating };

            _mockGameService.Setup(s => s.CreateRating(userId, ratingData)).ReturnsAsync(createdRating);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) }
            };

            // Act
            var result = await _controller.CreateRating(ratingData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(createdRating, okResult.Value);
        }

        [Fact]
        public async Task DeleteRating_ReturnsNoContent_WhenRatingIsDeleted()
        {
            // Arrange
            var deleteRatingData = new DeleteRatingDto { ProductId = 1 };
            var userId = Guid.NewGuid();

            _mockGameService.Setup(s => s.DeleteRating(userId, deleteRatingData)).Returns(Task.CompletedTask);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) }
            };

            // Act
            var result = await _controller.DeleteRating(deleteRatingData);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRating_ReturnsNotFound_WhenRatingDoesNotExist()
        {
            // Arrange
            var deleteRatingData = new DeleteRatingDto { ProductId = 1 };
            var userId = Guid.NewGuid();

            _mockGameService.Setup(s => s.DeleteRating(userId, deleteRatingData))
                            .ThrowsAsync(new MyApplicationException(ErrorStatus.NotFound, "Rating not found"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) }
            };

            // Act
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _controller.DeleteRating(deleteRatingData));

            // Assert
            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
            Assert.Equal("Rating not found", exception.Message);
        }

        [Fact]
        public async Task ListGames_ReturnsOkResult_WithGameList()
        {
            // Arrange
            var queryData = new ProductQueryDto { Genre = "Action", SortBy = "Price", SortDirection = "Asc" };

            List<SearchResultDto> products = new List<SearchResultDto>
            {
                new SearchResultDto { Id = 1, Name = "Game1", Genre = "Action"},
                new SearchResultDto { Id = 2, Name = "Game2", Genre = "Action"}
            };

            var productsResult = new ProductListResultDto { Products = products, Page = 1, PageSize = 2, TotalItems = 2 };


            _mockGameService.Setup(s => s.ListGames(queryData)).ReturnsAsync(productsResult);

            // Act
            var result = await _controller.ListGames(queryData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(productsResult, okResult.Value);
        }

        [Fact]
        public async Task ListGames_ReturnsOkResult_WithEmptyList_WhenNoGamesFound()
        {
            // Arrange
            var queryData = new ProductQueryDto { Genre = "NonExistentGenre" };
            var emptyProductList = new ProductListResultDto { Products = new List<SearchResultDto>(), Page = 1, PageSize = 10, TotalItems = 0 };

            _mockGameService.Setup(s => s.ListGames(queryData)).ReturnsAsync(emptyProductList);

            // Act
            var result = await _controller.ListGames(queryData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Empty(((ProductListResultDto)okResult.Value).Products);
        }
    }
}
