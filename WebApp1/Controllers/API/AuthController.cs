using Business.Exceptions;
using Business.Intefraces;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
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
        private readonly IAuthService _authService;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, IEmailService emailService, IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _emailService = emailService;
            _authService = authService;
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn([FromBody] LoginModel model)
        {
            try
            {
                var token = await _authService.SignInAsync(model);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            (bool success, string userId, string token, IEnumerable<string> errors) = await _authService.SignUpAsync(request);

            if (!success)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Sign-up failed due to invalid data.")
                {
                    Data = { { "Errors", errors } }
                };
            }

            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            return Created("", new { userId, encodedToken });
        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (!result.Succeeded)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Email confirmation failed.")
                {
                    Data = { { "Errors", result.Errors.Select(e => e.Description) } }
                };
            }

            return NoContent();
        }
    }
}
