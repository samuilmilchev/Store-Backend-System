using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Text.Json;
using WebApp1.Models;

namespace WebApp1.Middleware
{
    public static class ExceptionHandlerMiddleware
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature != null)
                    {
                        var ex = exceptionHandlerPathFeature.Error;

                        // Log the global exception as 'Error' with the stack trace
                        Log.Error(ex, "Global exception caught: {Message}, Path: {Path}, Method: {Method}, StackTrace: {StackTrace}",
                                  ex.Message, context.Request.Path, context.Request.Method, ex.StackTrace);

                        context.Response.StatusCode = ex switch
                        {
                            InvalidOperationException _ => StatusCodes.Status400BadRequest,
                            UnauthorizedAccessException _ => StatusCodes.Status401Unauthorized,
                            _ => StatusCodes.Status500InternalServerError // Default for unknown exceptions
                        };

                        // Create the custom error response object
                        string message;
                        string stackTrace;

                        if (env.IsDevelopment())
                        {
                            message = ex.Message;
                            stackTrace = ex.StackTrace; // Show stack trace in development only
                        }
                        else
                        {
                            message = "An unexpected error occurred. Please contact support.";
                            stackTrace = null; // Don't expose stack trace in production
                        }

                        var customErrorResponse = new CustomErrorResponse(message, stackTrace);

                        // Set the response content type to JSON
                        context.Response.ContentType = "application/json";

                        // Serialize the custom error response to JSON and write it to the response
                        var jsonResponse = JsonSerializer.Serialize(customErrorResponse);
                        await context.Response.WriteAsync(jsonResponse);
                    }
                });
            });
        }
    }
}
