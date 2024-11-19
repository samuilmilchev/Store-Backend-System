namespace Shared.DTOs
{
    public class RemoveOrderItemsDto
    {
        public int OrderId { get; set; }
        public List<int> ProductIds { get; set; }
    }
}
