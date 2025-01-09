using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Shared.DTOs;
using Shared.Enums;

namespace Business.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrdersRepository _orderRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public OrdersService(IOrdersRepository orderRepository, IGameRepository gameRepository, IUserService userService, IMapper mapper)
        {
            _gameRepository = gameRepository;
            _userService = userService;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<Order> GetOrderById(int id)
        {
            var order = await _orderRepository.GetOrderById(id);

            if (order == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Order with id {id} does not exist.");
            }

            return order;
        }

        public async Task<OrderResponseDto> CreateOrder(Guid userId, CreateOrderDto orderData)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            var order = await CreateOrederFromData(user, orderData);
            user.Orders.Add(order);

            await _orderRepository.CreateOrder(order);

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<List<OrderResponseDto>> GetOrders(Guid userId, int orderId)
        {
            if (orderId > 0)
            {
                var order = await _orderRepository.GetOrderById(orderId);
                List<Order> orders = new List<Order> { order };

                return _mapper.Map<List<OrderResponseDto>>(orders);
            }

            var user = await _userService.GetUserByIdAsync(userId);

            return _mapper.Map<List<OrderResponseDto>>(user.Orders);
        }

        public async Task<OrderResponseDto> UpdateOrder(Guid userId, UpdateOrderItemsDto orderData)
        {
            var order = await _orderRepository.GetOrderById(orderData.OrderId);

            if (order == null || order.UserId != userId || order.IsPaid)
            {
                throw new MyApplicationException(ErrorStatus.InvalidOperation, "Can not update this order.");
            }

            foreach (var itemUpdate in orderData.Items)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.ProductId == itemUpdate.ProductId);
                if (orderItem != null)
                    orderItem.Quantity = itemUpdate.Quantity;
            }

            await _orderRepository.UpdateOrder(order);

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task DeleteOrderItems(Guid userId, RemoveOrderItemsDto orderData)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            var order = await _orderRepository.GetOrderById(orderData.OrderId);

            if (order == null || order.UserId != userId || order.IsPaid)
            {
                throw new MyApplicationException(ErrorStatus.InvalidOperation, "Can not remove items from this order.");
            }

            foreach (var id in orderData.ProductIds)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.ProductId == id);
                if (orderItem != null)
                    order.Items.Remove(orderItem);
            }

            await _orderRepository.DeleteOrder(user, order);

            return;
        }

        public async Task BuyItems(Guid userId, int orderId)
        {
            var order = await _orderRepository.GetOrderById(orderId);

            if (order == null || order.UserId != userId || order.IsPaid)
            {
                throw new MyApplicationException(ErrorStatus.InvalidOperation, "Can not complete this order.");
            }

            order.IsPaid = true;
            order.Status = OrderStatus.Completed;

            await _orderRepository.BuyOrder(order);

            return;
        }

        private async Task<Order> CreateOrederFromData(ApplicationUser user, CreateOrderDto orderData)
        {
            Order order = new Order
            {
                User = user,
                UserId = user.Id
            };

            foreach (var item in orderData.Items)
            {
                var product = await _gameRepository.SearchGameByIdAsync(item.ProductId);

                OrderItem orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity,
                    Price = product.Price,
                };

                order.Items.Add(orderItem);
            }

            return order;
        }
    }
}
