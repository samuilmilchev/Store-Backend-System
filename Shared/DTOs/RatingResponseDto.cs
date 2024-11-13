namespace Shared.DTOs
{
    public class RatingResponseDto
    {
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
    }
}
