using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace WebApp1.Middleware
{
    public static class ExceptionHandlerMiddleware
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app)
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

                        // Send a user-friendly response message
                        var responseMessage = context.Response.StatusCode == StatusCodes.Status500InternalServerError
                            ? "An unexpected error occurred. Please contact support."
                            : "An unexpected error occurred. Please try again later.";

                        await context.Response.WriteAsync(responseMessage);
                    }
                });
            });
        }
    }
}
