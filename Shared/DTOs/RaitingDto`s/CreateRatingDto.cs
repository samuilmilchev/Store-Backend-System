using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class CreateRatingDto
    {
        public int ProductId { get; set; }

        [Range(1, 10, ErrorMessage = "Rating must be between 1 and 10.")]
        public int Rating { get; set; }
    }
}
