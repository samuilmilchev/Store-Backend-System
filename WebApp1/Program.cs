using DAL.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
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

            // Add services to the container
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register ApplicationDbContext with Identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add ASP.NET Core Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();  // Token provider is required for password reset, email confirmation, etc.

            // Configure JWT Authentication
            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddHealthChecks();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Virtual Wallet API V1", Version = "v1" });
            });

            //        // Add services to the container.
            //        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //            options.UseSqlServer(connectionString));
            //        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //            .AddEntityFrameworkStores<ApplicationDbContext>();
            //        builder.Services.AddRazorPages();

            //        builder.Services.AddHealthChecks()
            //.AddCheck("SQL Connection Health Check",
            //          new SqlConnectionHealthCheck(connectionString),
            //          HealthStatus.Unhealthy,
            //          tags: new[] { "sql" });

            var app = builder.Build();

            app.UseCustomExceptionHandler(app.Environment);

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
            app.UseAuthentication();
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
