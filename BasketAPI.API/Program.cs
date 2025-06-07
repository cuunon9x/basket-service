using BasketAPI.Infrastructure.Configuration;
using BasketAPI.API.Configuration;
using BasketAPI.API.HealthChecks;
using BasketAPI.API.Middleware;
using BasketAPI.Application.Common.Behaviors;
using BasketAPI.Application.Common.Mappings;
using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Add configurations
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
// Note: gRPC client for Discount service is disabled as the service is not part of this containerized setup
// builder.Services.AddGrpcConfiguration(builder.Configuration);
// Enable MassTransit for RabbitMQ message publishing
builder.Services.AddMassTransitConfiguration(builder.Configuration);

// Add Infrastructure services (including Redis)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ValidationBehavior<,>).Assembly);
});

// Add Validation and Behaviors
builder.Services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

// Add MediatR Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// Add Mapster configuration
builder.Services.AddApplicationMappings();

// Add Carter for minimal APIs
builder.Services.AddCarter();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = "60 seconds"
        });
    };
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: new[] { "ready", "db" },
        timeout: TimeSpan.FromSeconds(5));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BasketCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API V1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("BasketCorsPolicy");

// Add metrics
app.UseMetricServer();
app.UseHttpMetrics();

// Add request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Add custom metrics
app.UseMiddleware<MetricsMiddleware>();

// Add global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRateLimiter();
app.UseAuthorization();

// Configure health check endpoints
app.MapHealthChecks("/health", new()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Map Carter endpoints
app.MapCarter();

app.Run();
