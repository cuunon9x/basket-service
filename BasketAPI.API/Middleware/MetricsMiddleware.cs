using Prometheus;

namespace BasketAPI.API.Middleware;

public class MetricsMiddleware
{
    private readonly Counter _requestCounter;
    private readonly Histogram _requestDuration;
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
        _requestCounter = Metrics.CreateCounter("basket_api_requests_total", "HTTP Requests Total",
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status" }
            });

        _requestDuration = Metrics.CreateHistogram("basket_api_request_duration_seconds",
            "HTTP Request Duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode.ToString();

        using var timer = _requestDuration
            .WithLabels(new[] { method, path })
            .NewTimer();

        try
        {
            await _next(context);
        }
        finally
        {
            _requestCounter
                .WithLabels(new[] { method, path, statusCode })
                .Inc();
        }
    }
}
