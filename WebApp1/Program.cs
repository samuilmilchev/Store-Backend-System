using Business.Intefraces;
using Business.Mappings;
using Business.Services;
using DAL.Data;
using DAL.Entities;
using DAL.Repository;
using DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;
using WebApp1.Configurations;
using WebApp1.Middleware;
using WebApp1.Services;

namespace WebApp1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("DAL")));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddScoped<IGameRepository, GameRepository>();
            builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<IImagesService, ImagesService>();
            builder.Services.AddScoped<IOrdersService, OrdersService>();
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };
            });

            builder.Services.AddHealthChecks();
            builder.Services.AddRazorPages();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(UserProfile));
            builder.Services.AddAutoMapper(typeof(ProductProfile));
            builder.Services.AddAutoMapper(typeof(RatingProfile));
            builder.Services.AddAutoMapper(typeof(OrderProfile));
            builder.Services.AddMemoryCache();

            builder.Services.AddSwaggerDocumentation();

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "application/xml", "text/plain" });
            });

            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });


            //        builder.Services.AddHealthChecks()
            //.AddCheck("SQL Connection Health Check",
            //          new SqlConnectionHealthCheck(connectionString),
            //          HealthStatus.Unhealthy,
            //          tags: new[] { "sql" });

            var app = builder.Build();

            var jwtSettings = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(jwtSettings);

            if (!Validator.TryValidateObject(jwtSettings, validationContext, validationResults, true))
            {
                throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
            }

            app.UseResponseCompression();

            app.UseCustomExceptionHandler(app.Environment);

            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                // Comment out this line to test your global handler
                // app.UseDeveloperExceptionPage(); 
            }
            else
            {
                app.UseHsts();
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

            using (var scope = app.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;

                var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
                ApplicationDbContextFactory.ApplyPendingMigrations(dbContext)
                                           .GetAwaiter()
                                           .GetResult();

                await SeedRoles.Initialize(scopedServices);
                await SeedRoles.InitializeProducts(scopedServices);
            }

            app.Run();

            Log.CloseAndFlush();
        }
    }
}