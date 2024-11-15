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
        public DbSet<ProductRating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductRating>().HasKey(pr => pr.Id);

            modelBuilder.Entity<ProductRating>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Ratings)
                .HasForeignKey(pr => pr.ProductId);

            modelBuilder.Entity<ProductRating>()
                .HasOne(pr => pr.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(pr => pr.UserId);


            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Platform);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.DateCreated);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.TotalRating);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Genre);
        }
    }
}
