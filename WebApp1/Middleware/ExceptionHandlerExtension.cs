using Microsoft.AspNetCore.Mvc;

namespace WebApp1.Middleware // Adjust the namespace as necessary
{
    public static class ExceptionHandlerExtension
    {
        private static readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers = new()
        {
            // Add your specific exception handlers here
            { typeof(MyApplicationException), HandleMyApplicationException },
            { typeof(OperationCanceledException), HandleOperationCanceledException },
            // Add more specific handlers as needed
        };

        private static async Task HandleMyApplicationException(HttpContext httpContext, Exception ex)
        {
            var exception = (MyApplicationException)ex;
            httpContext.Response.StatusCode = exception.ErrorStatus switch
            {
                ErrorStatus.InvalidData => StatusCodes.Status400BadRequest,
                ErrorStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorStatus.Forbidden => StatusCodes.Status403Forbidden,
                ErrorStatus.NotFound => StatusCodes.Status404NotFound,
                ErrorStatus.NotAcceptable => StatusCodes.Status406NotAcceptable,
                ErrorStatus.PayloadLarge => StatusCodes.Status413RequestEntityTooLarge,
                _ => throw ex
            };

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Detail = exception.Message,
                Status = httpContext.Response.StatusCode,
            });
        }

        private static async Task HandleOperationCanceledException(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Detail = "Your submission was canceled.",
                Status = httpContext.Response.StatusCode,
            });
        }

        private static async Task HandleUnknownException(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var problemDetails = new ProblemDetails()
            {
                Detail = "Something went wrong...",
                Status = httpContext.Response.StatusCode,
            };
            problemDetails.Title = exception.Message; // Show the message in development
            problemDetails.Detail = exception.StackTrace; // Show the stack trace in development
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
        }

        public static void HandleExceptions(this IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage(); // Enable detailed error pages in development
            app.Use(async (httpContext, next) =>
            {
                try
                {
                    await next(); // Execute the next middleware
                }
                catch (Exception exception)
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILogger<Exception>>();
                    logger.LogError(exception, exception.Message); // Log the exception

                    var exceptionType = exception.GetType();
                    if (_exceptionHandlers.ContainsKey(exceptionType))
                        await _exceptionHandlers[exceptionType].Invoke(httpContext, exception); // Invoke the specific handler
                    else
                        await HandleUnknownException(httpContext, exception); // Handle unknown exceptions
                }
            });
        }
    }
}