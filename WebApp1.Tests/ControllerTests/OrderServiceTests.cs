using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using Business.Services;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Moq;
using Shared.DTOs;
using Shared.Enums;

namespace WebApp1.Tests.ControllerTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrdersRepository> _mockOrdersRepository;
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly OrdersService _ordersService;

        public OrderServiceTests()
        {
            _mockOrdersRepository = new Mock<IOrdersRepository>();
            _mockGameRepository = new Mock<IGameRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();
            _ordersService = new OrdersService(
                _mockOrdersRepository.Object,
                _mockGameRepository.Object,
                _mockUserService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var mockOrder = new Order { Id = orderId };
            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync(mockOrder);

            // Act
            var result = await _ordersService.GetOrderById(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(orderId), Times.Once);
        }

        [Fact]
        public async Task GetOrderById_ThrowsMyApplicationException_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 1;

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync((Order)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _ordersService.GetOrderById(orderId));

            Assert.Equal(ErrorStatus.NotFound, exception.ErrorStatus);
            Assert.Equal($"Order with id {orderId} does not exist.", exception.Message);

            _mockOrdersRepository.Verify(repo => repo.GetOrderById(orderId), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ReturnsOrderResponseDto_WhenOrderIsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<UpdateOrderDto>
                {
                    new UpdateOrderDto { ProductId = 1, Quantity = 2 }
                }
            };
            var mockUser = new ApplicationUser { Id = userId, Orders = new List<Order>() };
            var mockOrder = new Order { Id = 1 };
            var expectedResponse = new OrderResponseDto { Id = mockOrder.Id };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(mockUser);
            _mockGameRepository.Setup(repo => repo.SearchGameByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Product { Id = 1, Price = 10 });
            _mockOrdersRepository.Setup(repo => repo.CreateOrder(It.IsAny<Order>())).ReturnsAsync(mockOrder);
            _mockMapper.Setup(mapper => mapper.Map<OrderResponseDto>(It.IsAny<Order>())).Returns(expectedResponse);

            // Act
            var result = await _ordersService.CreateOrder(userId, createOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            _mockOrdersRepository.Verify(repo => repo.CreateOrder(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task GetOrders_ReturnsMappedOrders_WhenOrdersExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = 0;
            var mockUser = new ApplicationUser
            {
                Id = userId,
                Orders = new List<Order> { new Order { Id = 1 }, new Order { Id = 2 } }
            };
            var expectedResponse = new List<OrderResponseDto>
            {
                new OrderResponseDto { Id = 1 },
                new OrderResponseDto { Id = 2 }
            };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(mockUser);
            _mockMapper.Setup(mapper => mapper.Map<List<OrderResponseDto>>(mockUser.Orders)).Returns(expectedResponse);

            // Act
            var result = await _ordersService.GetOrders(userId, orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockMapper.Verify(mapper => mapper.Map<List<OrderResponseDto>>(mockUser.Orders), Times.Once);
        }

        [Fact]
        public async Task GetOrders_ReturnsUserOrders_WhenOrderIdNotProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            int orderId = 0;

            var orders = new List<Order>
            {
                new Order { Id = 1, UserId = userId, Items = new List<OrderItem>() },
                new Order { Id = 2, UserId = userId, Items = new List<OrderItem>() }
            };

            var user = new ApplicationUser
            {
                Id = userId,
                Orders = orders
            };

            var expectedResponse = new List<OrderResponseDto>
            {
                new OrderResponseDto { Id = 1 },
                new OrderResponseDto { Id = 2 }
            };

            _mockUserService
                .Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            _mockMapper
                .Setup(mapper => mapper.Map<List<OrderResponseDto>>(orders))
                .Returns(expectedResponse);

            // Act
            var result = await _ordersService.GetOrders(userId, orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedResponse, result);

            _mockUserService.Verify(service => service.GetUserByIdAsync(userId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<List<OrderResponseDto>>(orders), Times.Once);
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsOrderResponseDto_WhenOrderIsUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateOrderItemsDto = new UpdateOrderItemsDto
            {
                OrderId = 1,
                Items = new List<UpdateOrderDto> { new UpdateOrderDto { ProductId = 1, Quantity = 5 } }
            };
            var mockOrder = new Order { Id = 1, UserId = userId, Items = new List<OrderItem>() };
            var expectedResponse = new OrderResponseDto { Id = mockOrder.Id };

            _mockOrdersRepository.Setup(repo => repo.GetOrderById(updateOrderItemsDto.OrderId)).ReturnsAsync(mockOrder);
            _mockOrdersRepository.Setup(repo => repo.UpdateOrder(mockOrder)).ReturnsAsync(mockOrder);
            _mockMapper.Setup(mapper => mapper.Map<OrderResponseDto>(mockOrder)).Returns(expectedResponse);

            // Act
            var result = await _ordersService.UpdateOrder(userId, updateOrderItemsDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            _mockOrdersRepository.Verify(repo => repo.UpdateOrder(mockOrder), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_UpdatesOnlyMatchingItemsInOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateData = new UpdateOrderItemsDto
            {
                OrderId = 1,
                Items = new List<UpdateOrderDto>
                {
                    new UpdateOrderDto { ProductId = 1, Quantity = 5 },
                    new UpdateOrderDto { ProductId = 999, Quantity = 3 }
                }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = userId,
                IsPaid = false,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2 },
                    new OrderItem { ProductId = 2, Quantity = 4 }
                }
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(updateData.OrderId))
                .ReturnsAsync(mockOrder);

            _mockOrdersRepository
                .Setup(repo => repo.UpdateOrder(mockOrder))
                .ReturnsAsync(mockOrder);

            // Act
            var result = await _ordersService.UpdateOrder(userId, updateData);

            // Assert
            Assert.Single(mockOrder.Items.Where(item => item.ProductId == 1 && item.Quantity == 5));
            Assert.Single(mockOrder.Items.Where(item => item.ProductId == 2 && item.Quantity == 4));
            _mockOrdersRepository.Verify(repo => repo.UpdateOrder(mockOrder), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ThrowsInvalidOperationException_WhenOrderIsPaid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateData = new UpdateOrderItemsDto
            {
                OrderId = 1,
                Items = new List<UpdateOrderDto> { new UpdateOrderDto { ProductId = 1, Quantity = 3 } }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = userId,
                IsPaid = true
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(updateData.OrderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.UpdateOrder(userId, updateData));
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(updateData.OrderId), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ThrowsInvalidOperationException_WhenOrderNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateData = new UpdateOrderItemsDto
            {
                OrderId = 2,
                Items = new List<UpdateOrderDto> { new UpdateOrderDto { ProductId = 1, Quantity = 3 } }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = userId,
                IsPaid = true
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(updateData.OrderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.UpdateOrder(userId, updateData));
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(updateData.OrderId), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ThrowsInvalidOperationException_WhenOrderUserIdNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateData = new UpdateOrderItemsDto
            {
                OrderId = 1,
                Items = new List<UpdateOrderDto> { new UpdateOrderDto { ProductId = 1, Quantity = 3 } }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = new Guid(),
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(updateData.OrderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.UpdateOrder(userId, updateData));
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(updateData.OrderId), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderItems_RemovesItemsFromOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var removeOrderItemsDto = new RemoveOrderItemsDto
            {
                OrderId = 1,
                ProductIds = new List<int> { 1 }
            };
            var mockOrder = new Order
            {
                Id = 1,
                UserId = userId,
                Items = new List<OrderItem> { new OrderItem { ProductId = 1 } }
            };
            var mockUser = new ApplicationUser { Id = userId, Orders = new List<Order> { mockOrder } };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(mockUser);
            _mockOrdersRepository.Setup(repo => repo.GetOrderById(removeOrderItemsDto.OrderId)).ReturnsAsync(mockOrder);

            // Act
            await _ordersService.DeleteOrderItems(userId, removeOrderItemsDto);

            // Assert
            Assert.Empty(mockOrder.Items);
            _mockOrdersRepository.Verify(repo => repo.DeleteOrder(mockUser, mockOrder), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderItems_ThrowsInvalidOperationException_WhenOrderIsPaid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var removeData = new RemoveOrderItemsDto
            {
                OrderId = 1,
                ProductIds = new List<int> { 1 }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = userId,
                IsPaid = true
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(removeData.OrderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.DeleteOrderItems(userId, removeData));
        }

        [Fact]
        public async Task DeleteOrderItems_ThrowsInvalidOperationException_WhenOrderUserNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var removeData = new RemoveOrderItemsDto
            {
                OrderId = 1,
                ProductIds = new List<int> { 1 }
            };

            var mockOrder = new Order
            {
                Id = 1,
                UserId = new Guid(),
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(removeData.OrderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.DeleteOrderItems(userId, removeData));
        }

        [Fact]
        public async Task BuyItems_UpdatesOrderStatusToCompleted_WhenOrderIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = 1;
            var mockOrder = new Order
            {
                Id = orderId,
                UserId = userId,
                IsPaid = false,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2 }
                }
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync(mockOrder);

            _mockOrdersRepository
                .Setup(repo => repo.BuyOrder(mockOrder))
                .ReturnsAsync(mockOrder);

            // Act
            await _ordersService.BuyItems(userId, orderId);

            // Assert
            Assert.True(mockOrder.IsPaid);
            Assert.Equal(OrderStatus.Completed, mockOrder.Status);
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(orderId), Times.Once);
            _mockOrdersRepository.Verify(repo => repo.BuyOrder(mockOrder), Times.Once);
        }

        [Fact]
        public async Task BuyItems_ThrowsException_WhenOrderDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = 1;

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync((Order)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() =>
                _ordersService.BuyItems(userId, orderId));

            Assert.Equal("Can not complete this order.", exception.Message);
            _mockOrdersRepository.Verify(repo => repo.GetOrderById(orderId), Times.Once);
            _mockOrdersRepository.Verify(repo => repo.BuyOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task BuyItems_ThrowsException_WhenOrderAlreadyPaid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = 1;

            var mockOrder = new Order
            {
                Id = orderId,
                UserId = userId,
                IsPaid = true,
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, Price = 50 },
                    new OrderItem { ProductId = 2, Quantity = 1, Price = 100 }
                }
            };

            _mockOrdersRepository
                .Setup(repo => repo.GetOrderById(orderId))
                .ReturnsAsync(mockOrder);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MyApplicationException>(() => _ordersService.BuyItems(userId, orderId));
            Assert.Equal(ErrorStatus.InvalidOperation, exception.ErrorStatus);
            Assert.Equal("Can not complete this order.", exception.Message);

            _mockOrdersRepository.Verify(repo => repo.BuyOrder(It.IsAny<Order>()), Times.Never);
        }
    }
}
