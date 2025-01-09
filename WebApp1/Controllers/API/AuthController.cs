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

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="model">Login model containing user credentials.</param>
        /// <returns>Returns a JWT token if the sign-in is successful.</returns>
        /// <response code="200">Returns the JWT token.</response>
        /// <response code="401">Unauthorized if the credentials are invalid.</response>
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

        /// <summary>
        /// Registers a new user and returns user ID and a token.
        /// </summary>
        /// <param name="request">Sign-up request model containing user details.</param>
        /// <returns>Returns the user ID and encoded JWT token if sign-up is successful.</returns>
        /// <response code="201">Returns the user ID and encoded token.</response>
        /// <response code="400">Bad request if sign-up fails due to invalid data.</response>
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

        /// <summary>
        /// Confirms a user's email address using a token.
        /// </summary>
        /// <param name="userId">The ID of the user whose email is to be confirmed.</param>
        /// <param name="token">The confirmation token sent to the user's email.</param>
        /// <returns>No content if the email confirmation is successful.</returns>
        /// <response code="204">No content if the email confirmation is successful.</response>
        /// <response code="400">Bad request if the confirmation fails.</response>
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
