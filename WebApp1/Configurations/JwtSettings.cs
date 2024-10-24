using System.ComponentModel.DataAnnotations;

namespace WebApp1.Configurations
{
    public class JwtSettings : IValidatableObject
    {
        [Required(ErrorMessage = "JWT Key is required.")]
        public string Key { get; set; }

        [Required(ErrorMessage = "JWT Issuer is required.")]
        public string Issuer { get; set; }

        [Required(ErrorMessage = "JWT Audience is required.")]
        public string Audience { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Expiration time must be greater than 0.")]
        public int ExpireMinutes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Key))
            {
                yield return new ValidationResult("JWT Key is required.", new[] { nameof(Key) });
            }

            if (string.IsNullOrEmpty(Issuer))
            {
                yield return new ValidationResult("JWT Issuer is required.", new[] { nameof(Issuer) });
            }

            if (string.IsNullOrEmpty(Audience))
            {
                yield return new ValidationResult("JWT Audience is required.", new[] { nameof(Audience) });
            }

            if (ExpireMinutes <= 0)
            {
                yield return new ValidationResult("Expiration time must be greater than 0.", new[] { nameof(ExpireMinutes) });
            }
        }
    }
}
