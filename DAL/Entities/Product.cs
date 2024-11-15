using Shared.Enums;

namespace DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Platforms Platform { get; set; }
        public DateTime DateCreated { get; set; }
        public double TotalRating { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public Rating Rating { get; set; }
        public string? Logo { get; set; }
        public string? Background { get; set; }
        public int Count { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
    }
}
