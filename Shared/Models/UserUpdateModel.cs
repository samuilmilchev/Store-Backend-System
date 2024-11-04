using Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    /// <summary>
    /// Represents the data required to update a user's profile.
    /// </summary>
    public class UserUpdateModel
    {
        /// <summary>
        /// Gets or sets the user's username.
        /// </summary>
        /// <remarks>
        /// Optional. This is the unique name used to identify the user within the system.
        /// Example: Tom
        /// </remarks>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// Required. This is the email address associated with the user's account.
        /// It must be in a valid email format.
        /// Example: tom.zui@example.com
        /// </remarks>
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        /// <remarks>
        /// Optional. This must be exactly 10 digits.
        /// Example: 0896584578
        /// </remarks>
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's delivery address.
        /// </summary>
        /// <remarks>
        /// Optional. This object contains address information for deliveries.
        /// </remarks>
        public UserAddressDTO? AddressDelivery { get; set; }
    }
}
