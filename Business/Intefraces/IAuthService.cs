using Microsoft.AspNetCore.Identity;
using Shared.Models;

namespace Business.Intefraces
{
    public interface IAuthService
    {
        Task<string> SignInAsync(LoginModel model);
        Task<(bool Success, string UserId, string Token, IEnumerable<string> Errors)> SignUpAsync(SignUpRequest request);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    }
}
