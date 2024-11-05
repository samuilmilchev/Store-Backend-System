using Business.Exceptions;
using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.Security.Claims;

namespace WebApp1.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _dbContext;
        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, SignInManager<ApplicationUser> signInManager, IUserService userService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userService = userService;
        }

        /// <summary>
        /// Updates user information.
        /// </summary>
        /// <param name="updateModel">User update model containing the new user information.</param>
        /// <returns>Returns a status of 200 OK if the update was successful.</returns>
        /// <response code="200">User information was updated successfully.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateModel updateModel)
        {
            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            await _userService.UpdateUserAsync(userId, updateModel);
            return Ok();
        }

        /// <summary>
        /// Updates the user's password.
        /// </summary>
        /// <param name="passwordUpdateModel">Model containing the old and new password.</param>
        /// <returns>Returns a status of 204 No Content if the password was updated successfully.</returns>
        /// <response code="204">Password updated successfully.</response>
        /// <response code="400">Bad request if the old password is incorrect or new password is invalid.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        [Authorize]
        [HttpPatch("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateModel passwordUpdateModel)
        {
            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            bool success = await _userService.UpdatePasswordAsync(userId, passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword);
            if (!success)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData);
            }

            return NoContent();
        }

        /// <summary>
        /// Retrieves the profile information for the logged-in user.
        /// </summary>
        /// <returns>Returns the user's profile information.</returns>
        /// <response code="200">Returns the user profile data.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.Claims
            .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            var userProfile = await _userService.GetUserProfileAsync(userId);

            return Ok(userProfile);
        }
    }
}
