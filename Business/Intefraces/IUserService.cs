using Shared.Models;

namespace Business.Intefraces
{
    public interface IUserService
    {
        Task UpdateUserAsync(Guid userId, UserUpdateModel updateModel);
        Task<bool> UpdatePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<UserProfileModel> GetUserProfileAsync(Guid userId);
    }
}
