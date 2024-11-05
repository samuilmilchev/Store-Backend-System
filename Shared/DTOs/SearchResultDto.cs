namespace Shared.DTOs
{
    public class SearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; }
        public DateTime DateCreated { get; set; }
        public double TotalRating { get; set; }
        public decimal Price { get; set; }
    }
}
