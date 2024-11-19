namespace Shared.DTOs
{
    public class UpdateOrderItemsDto
    {
        public int OrderId { get; set; }
        public List<UpdateOrderDto> Items { get; set; } = new List<UpdateOrderDto>();
    }
}
