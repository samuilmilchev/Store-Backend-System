using DAL.Data;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly ApplicationDbContext _context;

        public OrdersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderById(int id)
        {
            var order = _context.Orders
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.Id == id);

            return order;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();

            return order;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();

            return order;
        }

        public async Task<Order> DeleteOrder(ApplicationUser user, Order order)
        {
            if (order.Items.Count == 0)
            {
                user.Orders.Remove(order);
                _context.Orders.Remove(order);
            }

            _context.SaveChanges();

            return order;
        }

        public async Task<Order> BuyOrder(Order order)
        {
            _context.SaveChanges();

            return order;
        }
    }
}
