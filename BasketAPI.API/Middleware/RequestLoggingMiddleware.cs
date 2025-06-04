using System.Diagnostics;

namespace BasketAPI.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await LogRequest(context);
            await _next(context);
        }
        finally
        {
            sw.Stop();
            await LogResponse(context, sw.ElapsedMilliseconds);
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        _logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} started - Body: {RequestBody}",
            context.Request.Method,
            context.Request.Path,
            requestBody);
    }

    private Task LogResponse(HttpContext context, long elapsedMs)
    {
        _logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} finished in {ElapsedMilliseconds}ms with status code {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            elapsedMs,
            context.Response.StatusCode);

        return Task.CompletedTask;
    }
}
