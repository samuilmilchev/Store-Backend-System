namespace DAL.Entities
{
    public enum Platforms
    {
        Windows = 1,
        Mac = 2,
        Linux = 3,
        Mobile = 4
    }
    public enum Rating
    {
        Everyone = 1,
        Teen = 2,
        Mature = 3,
        Adult = 4
    }

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
    }
}
