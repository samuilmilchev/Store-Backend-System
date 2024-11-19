using DAL.Entities;
using Shared.DTOs;

namespace Business.Intefraces
{
    public interface IOrdersService
    {
        Task<Order> GetOrderById(int id);
        Task<OrderResponseDto> CreateOrder(Guid userId, CreateOrderDto orderData);
        Task<List<OrderResponseDto>> GetOrders(Guid userId, int orderId);
        Task<OrderResponseDto> UpdateOrder(Guid userId, UpdateOrderItemsDto orderData);
        Task DeleteOrderItems(Guid userId, RemoveOrderItemsDto orderData);
        Task BuyItems(Guid userId, int orderId);
    }
}
