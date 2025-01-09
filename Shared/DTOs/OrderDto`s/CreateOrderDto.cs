namespace Shared.DTOs
{
    public class CreateOrderDto
    {
        public List<UpdateOrderDto> Items { get; set; } = new List<UpdateOrderDto>();
    }
}
