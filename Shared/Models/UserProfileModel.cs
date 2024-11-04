namespace Shared.Models
{
    /// <summary>
    /// Represents the profile information of a user retrieved from the system.
    /// This model is used only for GET requests to provide user profile data.
    /// </summary>
    public class UserProfileModel
    {
        /// <summary>
        /// Gets the user's username.
        /// </summary>
        /// <remarks>
        /// Required. This is the unique name used to identify the user within the system.
        /// Example: Tom
        /// </remarks>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        /// <remarks>
        /// Required. This is the email address associated with the user's account.
        /// Example: tom.zui@example.com
        /// </remarks>
        public string Email { get; set; }

        /// <summary>
        /// Gets the user's phone number.
        /// </summary>
        /// <remarks>
        /// Optional. This is the contact number for the user.
        /// Example: 0896584578
        /// </remarks>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets the user's delivery address.
        /// </summary>
        /// <remarks>
        /// Optional. This is the address where deliveries can be sent.
        /// Example: ul.Vitosha, Sofia
        /// </remarks>
        public string AddressDelivery { get; set; }
    }
}
