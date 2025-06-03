using BasketAPI.Infrastructure.Configuration;
using Carter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Basket API", 
        Version = "v1",
        Description = "A shopping cart microservice with Redis and gRPC integration"
    });
});

// Add Infrastructure services (including Redis)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Carter for minimal APIs
builder.Services.AddCarter();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BasketCorsPolicy");
app.UseAuthorization();

// Map Carter endpoints
app.MapCarter();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
