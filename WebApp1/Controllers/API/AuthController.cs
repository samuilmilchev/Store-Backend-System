using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp1.Services;

namespace WebApp1.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;


        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            // Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the email format is valid
            if (!IsValidEmail(request.Email))
            {
                ModelState.AddModelError("Email", "The email format is invalid.");
                return BadRequest(ModelState);
            }

            //If user already exist
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { error = "User already exists." });
            }

            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Ensure the "User" role exists before assigning it
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    var roleCreationResult = await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });
                    if (!roleCreationResult.Succeeded)
                    {
                        // Return BadRequest if role creation fails
                        return BadRequest(new { Errors = roleCreationResult.Errors.Select(e => e.Description) });
                    }
                }

                // Assign the "User" role to the newly created user
                await _userManager.AddToRoleAsync(user, "User");
                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Generate confirmation link
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);

                try
                {
                    await _emailService.SendEmailConfirmation(request.Email, confirmationLink);
                }
                catch (Exception ex) // Catch the exception from the email service
                {
                    return BadRequest(new { error = "Failed to send email confirmation." });
                }

                // Generate email confirmation token
                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // For testing purposes, you can return the token in the response (but this should be sent via email in a real app)
                return Created("", new { userId = user.Id, emailConfirmationToken });
            }

            // Bad Request with error details
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        //private async Task SendEmailConfirmation(string userEmail, string confirmationLink)
        //{
        //    var email = _configuration["EmailSettings:Email"];
        //    var password = _configuration["EmailSettings:AppPassword"]; // Use the App Password here

        //    using (var client = new SmtpClient("smtp.gmail.com", 587)) // Port 587 for TLS
        //    {
        //        client.EnableSsl = true; // Ensure SSL is enabled
        //        client.Credentials = new System.Net.NetworkCredential(email, password);

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(email),
        //            Subject = "Confirm your email",
        //            Body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>",
        //            IsBodyHtml = true,
        //        };
        //        mailMessage.To.Add(userEmail);

        //        await client.SendMailAsync(mailMessage);

        //    }
        //}

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { error = "Invalid user ID." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new { message = "Email confirmed successfully!" });
            }

            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Get the roles assigned to the user
            var roles = await _userManager.GetRolesAsync(user);

            // Create claims, including roles
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Add user ID as a claim
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //[HttpGet("emailConfirm")]
        //public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //{
        //    // Find the user by ID
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        return BadRequest(new { error = "Invalid user ID." });
        //    }

        //    // Confirm the user's email using the provided token
        //    var result = await _userManager.ConfirmEmailAsync(user, token);

        //    if (result.Succeeded)
        //    {
        //        // Return 204 No Content on success
        //        return NoContent();
        //    }

        //    // Return 400 Bad Request with error details if confirmation fails
        //    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        //}

        // Method to validate email format
        private bool IsValidEmail(string email)
        {
            // Add your email validation logic here (e.g., using Regex)
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }

    public class SignUpRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }

}
