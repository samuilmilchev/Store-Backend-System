using DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class UserUpdateModel
    {
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }
        public UserAddress? AddressDelivery { get; set; }
    }
}
