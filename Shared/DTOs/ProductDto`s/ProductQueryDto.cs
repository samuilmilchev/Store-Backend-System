using Shared.Enums;

namespace Shared.DTOs
{
    public class ProductQueryDto
    {
        public string? Genre { get; set; }
        public int? Age { get; set; }
        public SortBy SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
