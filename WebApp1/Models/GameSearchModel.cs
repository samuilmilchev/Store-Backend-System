using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models
{
    public class GameSearchModel
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Term { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Limit must be a non-negative value.")]
        public int Limit { get; set; } = 10;

        [Range(0, int.MaxValue, ErrorMessage = "Offset must be a non-negative value.")]
        public int Offset { get; set; } = 0;
    }
}
