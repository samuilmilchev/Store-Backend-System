using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Text.RegularExpressions;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UserUpdateModel updateModel)
        {
            if (!IsValidEmail(updateModel.Email))
            {
                return false;
            }

            var user = await _userManager.Users
                .Include(u => u.AddressDelivery)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            user.UserName = updateModel.UserName;
            user.Email = updateModel.Email;
            user.PhoneNumber = updateModel.PhoneNumber;

            if (user.AddressDelivery != null)
            {
                if (updateModel.AddressDelivery != null)
                {
                    user.AddressDelivery = updateModel.AddressDelivery;
                }
            }
            else if (updateModel.AddressDelivery != null)
            {
                var address = new UserAddress
                {
                    UserId = user.Id,
                    AddressDelivery = updateModel.AddressDelivery.AddressDelivery
                };
                _dbContext.UserAddresses.Add(address);
            }

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdatePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<UserProfileModel> GetUserProfileAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.AddressDelivery)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserProfileModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AddressDelivery = user.AddressDelivery.AddressDelivery
            };
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                   Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
