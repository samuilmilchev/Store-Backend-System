namespace Shared.DTOs
{
    public class ProductQueryDto
    {
        public string? Genre { get; set; }
        public int? Age { get; set; }
        public string SortBy { get; set; } = "Rating";
        public string SortDirection { get; set; } = "Desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
