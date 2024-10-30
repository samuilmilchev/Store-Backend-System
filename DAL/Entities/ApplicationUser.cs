using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public UserAddress? AddressDelivery { get; set; }
    }
}
