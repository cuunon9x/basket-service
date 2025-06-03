# Basket API

A microservice for managing shopping carts with Redis caching and RabbitMQ integration, part of a larger microservices ecosystem.

## Overview

This Basket API is one component in a microservices architecture that includes:
- **Catalog API**: Manages product information (PostgreSQL)
- **Basket API** (this service): Handles shopping cart operations (Redis)
- **Discount API**: Manages product discounts (gRPC)
- **Ordering API**: Processes orders (SQL Server)


![b2](https://github.com/user-attachments/assets/a27bf81a-a511-4de0-b3d5-2ea9bc0ca984)

## Features

- Shopping cart CRUD operations
- Redis caching for high-performance data access
- Discount service integration via gRPC
- Asynchronous checkout processing with RabbitMQ
- Built with .NET 8 and Clean Architecture principles

## Prerequisites

- .NET 8 SDK
- Redis Server (for cart storage)
- RabbitMQ Server (for checkout message publishing)
- Discount Service (gRPC integration)
- Docker (optional)

## Getting Started

### Local Development

1. Clone the repository:
   ```powershell
   git clone https://github.com/yourusername/basket-api.git
   cd basket-api
   ```

2. Start required services:
   ```powershell
   # Start Redis (if using Docker)
   docker run -d -p 6379:6379 --name basket-redis redis

   # Start RabbitMQ (if using Docker)
   docker run -d -p 5672:5672 -p 15672:15672 --name basket-rabbitmq rabbitmq:3-management
   ```

3. Update configurations if needed:
   - Redis connection in `appsettings.json`
   - RabbitMQ settings in `appsettings.json`
   - Discount service gRPC endpoint in `appsettings.json`

4. Build and run:
   ```powershell
   dotnet build
   dotnet run --project BasketAPI.API
   ```

5. Test the API:
   - Swagger UI: http://localhost:5196/swagger
   - Health Check: http://localhost:5196/health

### Docker Deployment

1. Build the Docker image:
   ```powershell
   docker build -t basket-api .
   ```

2. Run the container:
   ```powershell
   docker run -d -p 5196:80 --name basket-api basket-api
   ```

## Architecture

### Solution Structure
```
BasketAPI.sln
├── BasketAPI.API            # API Layer, Carter endpoints
├── BasketAPI.Application    # Application Layer, CQRS handlers
├── BasketAPI.Domain         # Domain Layer, Entities
└── BasketAPI.Infrastructure # Infrastructure Layer
```

### Design Patterns & Principles
- **Clean Architecture**: Separation of concerns with layered architecture
- **CQRS Pattern**: Separate command and query operations
- **Domain-Driven Design**: Rich domain model with business logic
- **Repository Pattern**: Data access abstraction
- **Decorator Pattern**: Cross-cutting concerns like caching
- **Mediator Pattern**: In-process messaging with MediatR

### Key Components
- **Carter**: Minimal API endpoints organization
- **MassTransit**: Message broker abstraction for RabbitMQ
- **Redis**: Distributed caching and cart storage
- **gRPC**: High-performance discount service integration
- **FluentValidation**: Request validation
- **Mapster**: Object mapping

## API Endpoints

### Basket Operations
```http
GET /basket/{userName}     # Get user's basket
POST /basket/{userName}    # Update basket
DELETE /basket/{userName}  # Delete basket
POST /basket/checkout     # Process checkout
```

### Health Check
```http
GET /health              # Service health status
```

## Event Integration

### Published Events
- `BasketCheckoutEvent`: Published when a basket checkout is requested
  ```json
  {
    "userName": "string",
    "totalPrice": 0,
    "firstName": "string",
    "lastName": "string",
    "emailAddress": "string",
    "shippingAddress": "string",
    "paymentDetails": {}
  }
  ```

## Monitoring & Operations

- Health checks for:
  - Redis connection
  - RabbitMQ connection
  - Discount gRPC service
- Logging with structured logging
- Docker containerization
- API documentation with Swagger

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Configuration

### Application Settings
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DatabaseId": 0
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "DiscountGrpc": {
    "Url": "http://localhost:5003"
  }
}
```

### Environment Variables
The following environment variables can override the appsettings.json configuration:

- `Redis__ConnectionString`: Redis connection string
- `RabbitMQ__Host`: RabbitMQ host address
- `RabbitMQ__Username`: RabbitMQ username
- `RabbitMQ__Password`: RabbitMQ password
- `DiscountGrpc__Url`: Discount service gRPC endpoint

## Testing

### Unit Tests
```powershell
dotnet test BasketAPI.Tests.Unit
```

Covers:
- Command/Query handlers
- Domain logic
- Validation rules
- Service implementations

### Integration Tests
```powershell
dotnet test BasketAPI.Tests.Integration
```

Tests:
- Redis repository operations
- RabbitMQ message publishing
- gRPC service integration
- API endpoints

### Performance Tests
Located in `/tests/BasketAPI.Tests.Performance`:
- Load testing scenarios
- Redis cache performance
- Checkout flow throughput

## Monitoring

### Metrics
The service exposes the following metrics endpoints:

- `/metrics`: Prometheus metrics
- `/health`: Health check UI
- `/health/ready`: Readiness probe
- `/health/live`: Liveness probe

Key metrics:
- Request latency
- Cache hit/miss ratio
- Message broker queue length
- gRPC call duration

### Logging
Structured logging with Serilog:
- Request/response logging
- Error tracking
- Performance metrics
- Integration events

Log levels:
- `Debug`: Detailed debugging info
- `Information`: General application flow
- `Warning`: Handled errors
- `Error`: Unhandled exceptions

### Distributed Tracing
OpenTelemetry integration for:
- Request tracing
- Cache operations
- Message publishing
- gRPC calls

## Troubleshooting

### Common Issues

1. Redis Connection:
   ```
   Error: No connection could be made...
   Solution: Check Redis server status and connection string
   ```

2. RabbitMQ Connection:
   ```
   Error: Connection refused...
   Solution: Verify RabbitMQ service and credentials
   ```

3. gRPC Service:
   ```
   Error: Failed to connect to discount service...
   Solution: Ensure Discount service is running and endpoint is correct
   ```

### Debugging

1. Enable Debug Logging:
   ```json
   {
     "Serilog": {
       "MinimumLevel": {
         "Default": "Debug"
       }
     }
   }
   ```

2. Check Container Logs:
   ```powershell
   docker logs basket-api
   ```

3. Verify Service Dependencies:
   ```powershell
   curl http://localhost:5196/health
   ```

## Performance Optimization

### Caching Strategy
- TTL-based cache policy
- Background cache refresh
- Bulk operations support

### Resource Limits
- Redis connection pool: 100 connections
- RabbitMQ prefetch count: 250
- API rate limiting: 1000 req/min

### Scaling
The service can be scaled:
- Horizontally with multiple instances
- Vertically by increasing resources
- Cache cluster expansion

Recommended instance resources:
- CPU: 2 cores
- Memory: 2GB
- Network: 1Gbps

## Deployment

### Kubernetes

1. Apply configurations:
   ```powershell
   kubectl apply -f k8s/
   ```

2. Verify deployment:
   ```powershell
   kubectl get pods -l app=basket-api
   ```

### Docker Compose
```powershell
docker-compose up -d
```

Included services:
- Basket API
- Redis
- RabbitMQ
- Monitoring stack
