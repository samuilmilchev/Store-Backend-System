using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IMapper mapper)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task UpdateUserAsync(Guid userId, UserUpdateModel updateModel)
        {
            var user = await _userManager.Users
                .Include(u => u.AddressDelivery)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, "User not found");
            }

            _mapper.Map(updateModel, user);

            if (user.AddressDelivery == null && updateModel.AddressDelivery != null)
            {
                var newAddress = _mapper.Map<UserAddress>(updateModel.AddressDelivery);
                newAddress.UserId = user.Id;
                _dbContext.UserAddresses.Add(newAddress);
            }

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, "User not found");
            }

            if (oldPassword == newPassword)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "New password is same as old password.");
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData);
            }

            return result.Succeeded;
        }

        public async Task<UserProfileModel> GetUserProfileAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.AddressDelivery)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, "User not found");
            }

            return _mapper.Map<UserProfileModel>(user);
        }
    }
}
