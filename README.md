# Basket API

A high-performance microservice for managing shopping carts with Redis caching and RabbitMQ messaging integration.

## Overview

The Basket API provides shopping cart management capabilities with:

- **Redis**: High-performance cart storage and caching
- **RabbitMQ**: Asynchronous checkout event publishing
- **Clean Architecture**: CQRS pattern with MediatR
- **Docker**: Containerized deployment ready

![Architecture Diagram](https://github.com/user-attachments/assets/a27bf81a-a511-4de0-b3d5-2ea9bc0ca984)

## Quick Start

### Using Docker (Recommended)

1. **Start all services:**

   ```powershell
   docker-compose up -d
   ```

2. **Verify services are running:**

   ```powershell
   docker-compose ps
   ```

3. **Access the API:**
   - **API**: http://localhost:5002
   - **Swagger UI**: http://localhost:5002/swagger
   - **Health Check**: http://localhost:5002/health
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## API Testing Examples

### 1. Health Check

```powershell
# Check service health
Invoke-RestMethod -Uri "http://localhost:5002/health" -Method Get
```

### 2. Get Basket

```powershell
# Get basket for a user
Invoke-RestMethod -Uri "http://localhost:5002/basket/john" -Method Get
```

### 3. Create/Update Basket

```powershell
# Create a new basket with items
$basketData = @{
    userName = "john"
    items = @(
        @{
            productId = "prod-001"
            productName = "iPhone 14"
            unitPrice = 999.99
            quantity = 1
            color = "Blue"
        },
        @{
            productId = "prod-002"
            productName = "AirPods Pro"
            unitPrice = 249.99
            quantity = 2
            color = "White"
        }
    )
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5002/basket/john" `
    -Method Post `
    -Body $basketData `
    -ContentType "application/json"
```

### 4. Checkout Basket

```powershell
# Process checkout
$checkoutData = @{
    userName = "john"
    totalPrice = 1499.97
    firstName = "John"
    lastName = "Doe"
    emailAddress = "john.doe@example.com"
    shippingAddress = "123 Main St, City, State 12345"
    paymentMethod = "CreditCard"
    cardNumber = "****-****-****-1234"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/basket/checkout" `
    -Method Post `
    -Body $checkoutData `
    -ContentType "application/json"
```

### 5. Delete Basket

```powershell
# Delete user's basket
Invoke-RestMethod -Uri "http://localhost:5002/basket/john" -Method Delete
```

### Using curl (Alternative)

```bash
# Get basket
curl -X GET "http://localhost:5002/basket/john"

# Create basket
curl -X POST "http://localhost:5002/basket/john" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john",
    "items": [
      {
        "productId": "prod-001",
        "productName": "iPhone 14",
        "unitPrice": 999.99,
        "quantity": 1,
        "color": "Blue"
      }
    ]
  }'

# Checkout
curl -X POST "http://localhost:5002/basket/checkout" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john",
    "totalPrice": 999.99,
    "firstName": "John",
    "lastName": "Doe",
    "emailAddress": "john.doe@example.com",
    "shippingAddress": "123 Main St, City, State 12345"
  }'
```

## Architecture & Technology Stack

### Solution Structure

```
BasketAPI.sln
├── BasketAPI.API            # Carter minimal APIs, middleware
├── BasketAPI.Application    # CQRS handlers, validation
├── BasketAPI.Domain         # Entities, business logic
└── BasketAPI.Infrastructure # Redis, RabbitMQ, services
```

### Key Technologies

- **.NET 8**: Modern framework with performance improvements
- **Carter**: Minimal API framework for clean endpoints
- **MediatR**: CQRS pattern implementation
- **Redis**: High-performance caching and cart storage
- **MassTransit**: RabbitMQ message broker integration
- **FluentValidation**: Request validation rules
- **Serilog**: Structured logging

## Configuration

### Required Settings (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Environment Variables

Override configuration with environment variables:

- `ConnectionStrings__Redis`: Redis connection string
- `RabbitMQ__Host`: RabbitMQ host address
- `RabbitMQ__Username`: RabbitMQ username
- `RabbitMQ__Password`: RabbitMQ password

## Development

### Local Setup

```powershell
# Clone and build
git clone <repository-url>
cd basket-service
dotnet build

# Start dependencies
docker-compose up redis rabbitmq -d

# Run the API
dotnet run --project BasketAPI.API
```

### Testing

```powershell
# Run all tests
dotnet test

# Watch mode for development
dotnet test --watch
```

## Monitoring & Health Checks

### Health Endpoints

- `/health`: Overall service health
- `/health/ready`: Readiness probe (includes Redis)
- `/health/live`: Liveness probe

### Monitoring Features

- **Structured Logging**: Serilog with request/response tracking
- **Metrics**: Prometheus-compatible metrics endpoint
- **Health Checks**: Redis connectivity monitoring
- **Error Handling**: Global exception middleware

## Troubleshooting

### Common Issues

**Redis Connection Failed**

```powershell
# Check Redis status
docker logs basket-service-redis-1

# Test connection
redis-cli -h localhost -p 6379 ping
```

**RabbitMQ Connection Issues**

```powershell
# Check RabbitMQ status
docker logs basket-service-rabbitmq-1

# Access management UI
# http://localhost:15672 (guest/guest)
```

**Service Health Check**

```powershell
# Check overall health
Invoke-RestMethod http://localhost:5002/health

# Check container logs
docker-compose logs basketapi
```
