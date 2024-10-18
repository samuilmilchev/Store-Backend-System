using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace WebApp1.Middleware
{
    public static class ExceptionHandlerMiddleware
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app, IHostEnvironment env) // Accepting the environment as a parameter
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

                        string responseMessage;

                        if (env.IsDevelopment())
                        {
                            // In development, include the error message and stack trace
                            responseMessage = $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}";
                        }
                        else
                        {
                            // In production, use a user-friendly message
                            if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
                            {
                                responseMessage = "An unexpected error occurred. Please contact support.";
                            }
                            else
                            {
                                // For other error codes, return the original error message
                                responseMessage = ex.Message; // Keeps the original message for non-500 errors
                            }
                        }

                        await context.Response.WriteAsync(responseMessage);
                    }
                });
            });
        }
    }
}
