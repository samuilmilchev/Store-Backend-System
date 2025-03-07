﻿using Business.Intefraces;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp1.Services;

namespace Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<ApplicationRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<string> SignInAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = await GenerateJwtToken(user);

            return token;
        }

        public async Task<(bool Success, string UserId, string Token, IEnumerable<string> Errors)> SignUpAsync(SignUpRequest request)
        {
            if (!IsValidEmail(request.Email))
            {
                return (false, null, null, new[] { "The email format is invalid." });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return (false, null, null, new[] { "User already exists." });
            }

            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    var roleCreationResult = await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });
                    if (!roleCreationResult.Succeeded)
                    {
                        return (false, null, null, roleCreationResult.Errors.Select(e => e.Description));
                    }
                }

                await _userManager.AddToRoleAsync(user, "User");

                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName)
                    };
                await _userManager.AddClaimsAsync(user, claims);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = "";

                try
                {
                    await _emailService.SendEmailConfirmation(request.Email, confirmationLink);
                }
                catch (Exception)
                {
                    return (false, null, null, new[] { "Failed to send email confirmation." });
                }

                return (true, user.Id.ToString(), token, null);
            }

            return (false, null, null, result.Errors.Select(e => e.Description));
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid user ID." });
            }

            return await _userManager.ConfirmEmailAsync(user, token);
        }
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private bool IsValidEmail(string email)
        {
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
}
