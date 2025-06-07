using BasketAPI.Application.Common.Interfaces;
using BasketAPI.Infrastructure.Services.Grpc;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Services;

public class DiscountService : IDiscountService
{
    private readonly IDiscountGrpcService _grpcService;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(
        IDiscountGrpcService grpcService,
        ILogger<DiscountService> logger)
    {
        _grpcService = grpcService ?? throw new ArgumentNullException(nameof(grpcService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(decimal Amount, string Description)> GetDiscountAsync(string productName)
    {
        try
        {
            var coupon = await _grpcService.GetDiscount(productName);
            return (coupon.Amount, coupon.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discount for product {ProductName}", productName);
            return (0, string.Empty);
        }
    }
}
