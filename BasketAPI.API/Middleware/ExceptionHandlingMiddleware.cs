using System.Net;
using System.Text.Json;
using BasketAPI.Domain.Exceptions;

namespace BasketAPI.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = "Error",
            Message = GetUserFriendlyMessage(exception)
        };

        context.Response.StatusCode = GetStatusCode(exception);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => string.Join(", ", validationEx.Errors),
            NotFoundException notFoundEx => notFoundEx.Message,
            _ => "An error occurred processing your request."
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}
