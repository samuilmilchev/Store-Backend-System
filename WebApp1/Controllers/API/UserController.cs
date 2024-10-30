using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared;
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

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateModel updateModel)
        {
            if (!ModelState.IsValid || updateModel == null)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var success = await _userService.UpdateUserAsync(userId, updateModel);
            if (!success)
            {
                return NotFound();
            }

            return Ok();
        }

        [Authorize]
        [HttpPatch("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateModel passwordUpdateModel)
        {
            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (passwordUpdateModel.NewPassword == passwordUpdateModel.OldPassword)
            {
                ModelState.AddModelError("NewPassword", "New password cannot be the same as the old password.");
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(userIdClaim.Value);

            bool success = await _userService.UpdatePasswordAsync(userId, passwordUpdateModel.OldPassword, passwordUpdateModel.NewPassword);
            if (!success)
            {
                return BadRequest("Password update failed.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.Claims
            .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound();
            }

            return Ok(userProfile);
        }
    }
}
