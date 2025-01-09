using Shared.Enums;

namespace Shared.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public int Amount => Items.Sum(item => item.Quantity);
        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.Price);
        public bool IsPaid { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Guid UserId { get; set; }
        public Guid? AddressDeliveryId { get; set; }
    }
}
