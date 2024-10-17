using DAL.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApp1.HealthCheck;

namespace WebApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Set the default minimum log level
                .WriteTo.Console() // Log to the console for development
                .WriteTo.File(@"D:\Vention\WebApp1\logs\information-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File(@"D:\Vention\WebApp1\logs\warning-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .WriteTo.File(@"D:\Vention\WebApp1\logs\error-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

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

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature != null)
                    {
                        var ex = exceptionHandlerPathFeature.Error;

                        // Log the global exception as 'Error' with additional context
                        Log.Error(ex, "Global exception caught: {Message}, Path: {Path}, Method: {Method}",
                                  ex.Message, context.Request.Path, context.Request.Method);

                        context.Response.StatusCode = ex switch
                        {
                            InvalidOperationException _ => StatusCodes.Status400BadRequest,
                            UnauthorizedAccessException _ => StatusCodes.Status401Unauthorized,
                            _ => StatusCodes.Status500InternalServerError // Default for unknown exceptions
                        };

                        // Send a user-friendly response message
                        var responseMessage = "An unexpected error occurred. Please try again later.";
                        await context.Response.WriteAsync(responseMessage);
                    }
                });
            });

            // Ensure UseRouting is before UseAuthorization and other request handling middleware
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
