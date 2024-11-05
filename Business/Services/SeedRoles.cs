using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp1.Services
{
    public static class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

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

        public static async Task InitializeProducts(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var scopredService = scope.ServiceProvider;
            var dbContext = scopredService.GetRequiredService<ApplicationDbContext>();

            var haveSomethingInTable = dbContext.Products.Any();

            if (!haveSomethingInTable)
            {
                dbContext.Products.AddRange(new Product
                {
                    Name = "The Witcher 3: Wild Hunt",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2015, 5, 19),
                    TotalRating = 9.7,
                    Price = 39.99m
                },
                new Product
                {
                    Name = "Stardew Valley",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2016, 2, 26),
                    TotalRating = 9.5,
                    Price = 14.99m
                },
                new Product
                {
                    Name = "Counter-Strike: Global Offensive",
                    Platform = Platforms.Linux,
                    DateCreated = new DateTime(2012, 8, 21),
                    TotalRating = 9.0,
                    Price = 14.99m
                },
                new Product
                {
                    Name = "Fortnite",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2017, 7, 25),
                    TotalRating = 8.7,
                    Price = 0.00m
                },
                new Product
                {
                    Name = "Hades",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2020, 9, 17),
                    TotalRating = 9.8,
                    Price = 24.99m
                },
                new Product
                {
                    Name = "Celeste",
                    Platform = Platforms.Linux,
                    DateCreated = new DateTime(2018, 1, 25),
                    TotalRating = 9.6,
                    Price = 19.99m
                },
                new Product
                {
                    Name = "Animal Crossing: New Horizons",
                    Platform = Platforms.Mobile,
                    DateCreated = new DateTime(2020, 3, 20),
                    TotalRating = 9.3,
                    Price = 59.99m
                },
                new Product
                {
                    Name = "Apex Legends",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2019, 2, 4),
                    TotalRating = 8.5,
                    Price = 0.00m
                },
                new Product
                {
                    Name = "Minecraft",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2011, 11, 18),
                    TotalRating = 9.4,
                    Price = 26.95m
                },
                new Product
                {
                    Name = "DOOM Eternal",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2020, 3, 20),
                    TotalRating = 9.2,
                    Price = 59.99m
                }
                );
            }

            dbContext.SaveChanges();
        }
    }
}
