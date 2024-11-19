namespace Shared.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public SearchResultDto Product { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
