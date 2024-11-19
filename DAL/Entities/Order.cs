using Shared.Enums;

namespace DAL.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public int Amount => Items.Sum(item => item.Quantity);
        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.Price);
        public bool IsPaid { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid? AddressDeliveryId { get; set; }
        public UserAddress? AddressDelivery { get; set; }
    }
}
