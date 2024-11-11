using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DAL.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            var context = new ApplicationDbContext(optionsBuilder.Options);

            ApplyPendingMigrations(context).GetAwaiter().GetResult();

            return context;
        }

        public static async Task ApplyPendingMigrations(ApplicationDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
