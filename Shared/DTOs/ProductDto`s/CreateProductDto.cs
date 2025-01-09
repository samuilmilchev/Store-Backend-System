using Microsoft.AspNetCore.Http;
using Shared.Enums;

namespace Shared.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public Platforms Platform { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public Rating Rating { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public int Count { get; set; }
        public string? LogoUrl { get; set; }
        public string? BackgroundUrl { get; set; }
    }
}
