using DAL.Data;
using DAL.Entities;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace WebApp1.Tests.Repository_Tests
{
    public class OrderRepositoryTests
    {
        private readonly OrdersRepository _repository;
        private readonly ApplicationDbContext _context;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new OrdersRepository(_context);
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, Price = 50 }
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOrderById(order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateOrder_AddsOrderToDatabase()
        {
            // Arrange
            var order = new Order
            {
                Id = 2,
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 3, Price = 30 }
                }
            };

            // Act
            var createdOrder = await _repository.CreateOrder(order);
            var retrievedOrder = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);

            // Assert
            Assert.NotNull(retrievedOrder);
            Assert.Equal(order.Id, retrievedOrder.Id);
            Assert.Single(retrievedOrder.Items);
        }

        [Fact]
        public async Task UpdateOrder_UpdatesOrderInDatabase()
        {
            // Arrange
            var order = new Order
            {
                Id = 3,
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, Price = 50 }
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            order.Items.First().Quantity = 5;

            // Act
            var updatedOrder = await _repository.UpdateOrder(order);
            var retrievedOrder = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);

            // Assert
            Assert.NotNull(retrievedOrder);
            Assert.Equal(5, retrievedOrder.Items.First().Quantity);
        }

        [Fact]
        public async Task DeleteOrder_RemovesOrderFromDatabase_WhenNoItemsExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var order = new Order
            {
                Id = 4,
                UserId = user.Id,
                Items = new List<OrderItem>()
            };

            user.Orders.Add(order);
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            await _repository.DeleteOrder(user, order);
            var retrievedOrder = _context.Orders.FirstOrDefault(o => o.Id == order.Id);

            // Assert
            Assert.Null(retrievedOrder);
            Assert.Empty(user.Orders);
        }

        [Fact]
        public async Task DeleteOrder_DoesNotRemoveOrder_WhenItemsExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var order = new Order
            {
                Id = 5,
                UserId = user.Id,
                Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1, Price = 100 }
            }
            };

            user.Orders.Add(order);
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            await _repository.DeleteOrder(user, order);
            var retrievedOrder = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);

            // Assert
            Assert.NotNull(retrievedOrder);
            Assert.Single(retrievedOrder.Items);
        }

        [Fact]
        public async Task BuyOrder_SavesChanges_WhenCalled()
        {
            // Arrange
            var order = new Order
            {
                Id = 6,
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1, Price = 100 }
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            await _repository.BuyOrder(order);

            // Assert
            var retrievedOrder = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);
            Assert.NotNull(retrievedOrder);
            Assert.Equal(order.Id, retrievedOrder.Id);
        }
    }
}
