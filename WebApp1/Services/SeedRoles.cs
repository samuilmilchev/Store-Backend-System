using DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebApp1.Services
{
    public class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles if they do not exist
                    roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }
    }
}
