using BasketAPI.Infrastructure.Services.Grpc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BasketAPI.API.HealthChecks;

public class DiscountServiceHealthCheck : IHealthCheck
{
    private readonly IDiscountGrpcService _discountService;

    public DiscountServiceHealthCheck(IDiscountGrpcService discountService)
    {
        _discountService = discountService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get a discount for a test product to verify the gRPC connection
            await _discountService.GetDiscount("test_product");
            return HealthCheckResult.Healthy("Discount gRPC service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Discount gRPC service is unhealthy", ex);
        }
    }
}
