using DAL.Entities;

namespace DAL.Repository.Interfaces
{
    public interface IOrdersRepository
    {
        Task<Order> GetOrderById(int id);
        Task<Order> CreateOrder(Order order);
        Task<Order> UpdateOrder(Order order);
        Task<Order> DeleteOrder(ApplicationUser user, Order order);
        Task<Order> BuyOrder(Order order);
    }
}
