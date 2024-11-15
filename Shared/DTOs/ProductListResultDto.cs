namespace Shared.DTOs
{
    public class ProductListResultDto
    {
        public IEnumerable<SearchResultDto> Products { get; set; } = new List<SearchResultDto>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
