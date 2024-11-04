using DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for Product fields
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Platform);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.DateCreated);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.TotalRating);

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "The Witcher 3: Wild Hunt",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2015, 5, 19),
                    TotalRating = 9.7,
                    Price = 39.99m
                },
                new Product
                {
                    Id = 2,
                    Name = "Stardew Valley",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2016, 2, 26),
                    TotalRating = 9.5,
                    Price = 14.99m
                },
                new Product
                {
                    Id = 3,
                    Name = "Counter-Strike: Global Offensive",
                    Platform = Platforms.Linux,
                    DateCreated = new DateTime(2012, 8, 21),
                    TotalRating = 9.0,
                    Price = 14.99m
                },
                new Product
                {
                    Id = 4,
                    Name = "Fortnite",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2017, 7, 25),
                    TotalRating = 8.7,
                    Price = 0.00m
                },
                new Product
                {
                    Id = 5,
                    Name = "Hades",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2020, 9, 17),
                    TotalRating = 9.8,
                    Price = 24.99m
                },
                new Product
                {
                    Id = 6,
                    Name = "Celeste",
                    Platform = Platforms.Linux,
                    DateCreated = new DateTime(2018, 1, 25),
                    TotalRating = 9.6,
                    Price = 19.99m
                },
                new Product
                {
                    Id = 7,
                    Name = "Animal Crossing: New Horizons",
                    Platform = Platforms.Mobile,
                    DateCreated = new DateTime(2020, 3, 20),
                    TotalRating = 9.3,
                    Price = 59.99m
                },
                new Product
                {
                    Id = 8,
                    Name = "Apex Legends",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2019, 2, 4),
                    TotalRating = 8.5,
                    Price = 0.00m
                },
                new Product
                {
                    Id = 9,
                    Name = "Minecraft",
                    Platform = Platforms.Mac,
                    DateCreated = new DateTime(2011, 11, 18),
                    TotalRating = 9.4,
                    Price = 26.95m
                },
                new Product
                {
                    Id = 10,
                    Name = "DOOM Eternal",
                    Platform = Platforms.Windows,
                    DateCreated = new DateTime(2020, 3, 20),
                    TotalRating = 9.2,
                    Price = 59.99m
                }
            );
        }
    }
}
