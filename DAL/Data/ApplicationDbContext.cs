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
        public DbSet<Order> Orders { get; set; }

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

            modelBuilder.Entity<Order>().HasKey(o => o.Id);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.CreationDate)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.UserId)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>()
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.AddressDelivery)
                .WithMany()
                .HasForeignKey(o => o.AddressDeliveryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>().HasKey(oi => new { oi.ProductId, oi.OrderId });

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreationDate);
        }
    }
}
