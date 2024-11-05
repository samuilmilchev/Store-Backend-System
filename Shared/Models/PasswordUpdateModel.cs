using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    /// <summary>
    /// Represents the model for updating a user's password.
    /// </summary>
    public class PasswordUpdateModel
    {
        /// <summary>
        /// Gets or sets the user's old password.
        /// </summary>
        /// <remarks>
        /// Required. The old password must match the current password for verification.
        /// Example: OldP@ssw0rd!
        /// </remarks>
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password for the user.
        /// </summary>
        /// <remarks>
        /// Required. Must be at least 8 characters long and contain at least one uppercase letter, 
        /// one lowercase letter, one number, and one special character.
        /// Example: NewP@ssw0rd!
        /// </remarks>
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; }
    }
}
