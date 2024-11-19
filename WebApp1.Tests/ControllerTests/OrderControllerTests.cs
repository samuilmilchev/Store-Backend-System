using Business.Intefraces;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.DTOs;
using Shared.Enums;
using System.Security.Claims;
using WebApp1.Controllers.API;

namespace WebApp1.Tests.ControllerTests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrdersService> _mockOrdersService;
        private readonly OrdersController _controller;
        private readonly Guid _testUserId;
        public OrderControllerTests()
        {
            _mockOrdersService = new Mock<IOrdersService>();
            _controller = new OrdersController(_mockOrdersService.Object);
            _testUserId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
            };

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(userClaims);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.User).Returns(mockPrincipal.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
        }

        [Fact]
        public async Task GetOrderById_ReturnsOkResult_WithOrderDetails()
        {
            // Arrange
            var orderId = 1;
            var expectedOrder = new Order
            {
                Id = orderId,
                CreationDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                UserId = _testUserId
            };

            _mockOrdersService.Setup(s => s.GetOrderById(orderId)).ReturnsAsync(expectedOrder);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedOrder, okResult.Value);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedResult_WithOrderDetails()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<UpdateOrderDto>
                {
                    new UpdateOrderDto { ProductId = 1, Quantity = 2 },
                    new UpdateOrderDto { ProductId = 2, Quantity = 1 }
                }
            };

            var expectedUserId = _testUserId;

            var expectedOrder = new OrderResponseDto
            {
                Id = 1,
                CreationDate = DateTime.UtcNow,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2, Price = 19.99m },
                    new OrderItemDto { ProductId = 2, Quantity = 1, Price = 39.99m }
                },
                IsPaid = false,
                Status = OrderStatus.Pending,
                UserId = expectedUserId,
                AddressDeliveryId = Guid.NewGuid()
            };

            _mockOrdersService
                .Setup(s => s.CreateOrder(expectedUserId, It.IsAny<CreateOrderDto>()))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedOrder, createdResult.Value);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult_WithOrderList()
        {
            // Arrange
            var orders = new List<OrderResponseDto>
            {
                new OrderResponseDto
                {
                    Id = 1,
                    CreationDate = DateTime.UtcNow,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductId = 1, Quantity = 2, Price = 19.99m },
                        new OrderItemDto { ProductId = 2, Quantity = 1, Price = 39.99m }
                    },
                    IsPaid = false,
                    Status = OrderStatus.Pending,
                    UserId = Guid.NewGuid(),
                    AddressDeliveryId = Guid.NewGuid()
                },
                new OrderResponseDto
                {
                    Id = 2,
                    CreationDate = DateTime.UtcNow.AddHours(-1),
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductId = 3, Quantity = 3, Price = 9.99m }
                    },
                    IsPaid = true,
                    Status = OrderStatus.Completed,
                    UserId = Guid.NewGuid(),
                    AddressDeliveryId = Guid.NewGuid()
                }
            };

            _mockOrdersService
                .Setup(service => service.GetOrders(It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrders(0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOrders = Assert.IsAssignableFrom<List<OrderResponseDto>>(okResult.Value);

            Assert.Equal(orders.Count, returnedOrders.Count);
            Assert.Equal(orders[0].Id, returnedOrders[0].Id);
            Assert.Equal(orders[1].Id, returnedOrders[1].Id);
        }

        [Fact]
        public async Task UpdateOrderItems_ReturnsOkResult_WhenOrderIsUpdated()
        {
            // Arrange
            var userId = _testUserId;
            var updateOrderItemsDto = new UpdateOrderItemsDto
            {
                OrderId = 1,
                Items = new List<UpdateOrderDto>
                {
                    new UpdateOrderDto { ProductId = 1, Quantity = 3 },
                    new UpdateOrderDto { ProductId = 2, Quantity = 0 }
                }
            };

            _mockOrdersService
                .Setup(s => s.UpdateOrder(userId, It.IsAny<UpdateOrderItemsDto>()))
                .ReturnsAsync(new OrderResponseDto
                {
                    Id = updateOrderItemsDto.OrderId,
                    CreationDate = DateTime.UtcNow,
                    Items = new List<OrderItemDto>
                    {
                new OrderItemDto { ProductId = 1, Quantity = 3, Price = 19.99m }
                    },
                    IsPaid = false,
                    Status = OrderStatus.Pending,
                    UserId = userId,
                    AddressDeliveryId = Guid.NewGuid()
                });

            // Act
            var result = await _controller.UpdateOrderItems(updateOrderItemsDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal(updateOrderItemsDto.OrderId, updatedOrder.Id);
            Assert.Single(updatedOrder.Items);
            Assert.Equal(1, updatedOrder.Items.First().ProductId);
            Assert.Equal(3, updatedOrder.Items.First().Quantity);
        }

        [Fact]
        public async Task DeleteOrderItems_ReturnsOkResult_WithUpdatedOrderDetails()
        {
            // Arrange
            var userId = _testUserId;
            var orderId = 1;
            var productIdsToRemove = new List<int> { 1, 2 };

            var initialOrder = new Order
            {
                Id = orderId,
                UserId = userId,
                IsPaid = false,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, Price = 19.99m },
                    new OrderItem { ProductId = 2, Quantity = 1, Price = 39.99m },
                    new OrderItem { ProductId = 3, Quantity = 3, Price = 9.99m }
                }
            };

            var updatedOrderResponse = new OrderResponseDto
            {
                Id = orderId,
                CreationDate = DateTime.UtcNow,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 3, Quantity = 3, Price = 9.99m }
                },
                IsPaid = false,
                Status = OrderStatus.Pending,
                UserId = userId
            };

            _mockOrdersService
                .Setup(s => s.DeleteOrderItems(userId, It.IsAny<RemoveOrderItemsDto>()));

            var removeOrderItemsDto = new RemoveOrderItemsDto
            {
                OrderId = orderId,
                ProductIds = productIdsToRemove
            };

            // Act
            var result = await _controller.DeleteOrderItems(removeOrderItemsDto);

            // Assert
            var noContent = Assert.IsType<NoContentResult>(result);

            Assert.Equal(204, noContent.StatusCode);
        }

        [Fact]
        public async Task CompleteOrder_ReturnsOkResult_WhenOrderIsCompleted()
        {
            // Arrange
            var orderId = 1;
            var userId = _testUserId;

            _mockOrdersService
                .Setup(s => s.BuyItems(userId, orderId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BuyItems(orderId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}
