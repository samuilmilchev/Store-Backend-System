using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    /// <summary>
    /// Represents the model for user login credentials.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <remarks>
        /// Required. Must be a valid email format.
        /// Example: user@example.com
        /// </remarks>
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        /// <remarks>
        /// Required. Must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.
        /// Example: P@ssw0rd!
        /// </remarks>
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }
}
