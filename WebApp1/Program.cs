using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApp1.HealthCheck;
using WebApp1.Middleware;

namespace WebApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration) // Load from appsettings.json or appsettings.Development.json
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddControllers();

            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Virtual Wallet API V1", Version = "v1" });
            });

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            builder.Services.AddHealthChecks()
    .AddCheck("SQL Connection Health Check",
              new SqlConnectionHealthCheck(connectionString),
              HealthStatus.Unhealthy,
              tags: new[] { "sql" });

            var app = builder.Build();

            app.UseCustomExceptionHandler();

            app.UseRouting();

            // Use this condition to set the appropriate error handling
            if (app.Environment.IsDevelopment())
            {
                // Comment out this line to test your global handler
                // app.UseDeveloperExceptionPage(); 
            }
            else
            {
                app.UseHsts(); // Use HSTS for production
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApp1 API V1");
                options.RoutePrefix = "swagger";
            });

            app.MapHealthChecks("/hc");
            app.MapRazorPages();
            app.MapControllers();

            app.Run();

            Log.CloseAndFlush();
        }
    }
}
