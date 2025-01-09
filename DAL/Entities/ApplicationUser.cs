using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public UserAddress? AddressDelivery { get; set; }
        public ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
