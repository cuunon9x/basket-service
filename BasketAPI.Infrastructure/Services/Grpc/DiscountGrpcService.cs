using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Services.Grpc;

public class DiscountGrpcService : IDiscountGrpcService
{
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;
    private readonly ILogger<DiscountGrpcService> _logger;

    public DiscountGrpcService(
        DiscountProtoService.DiscountProtoServiceClient discountProtoService,
        ILogger<DiscountGrpcService> logger)
    {
        _discountProtoService = discountProtoService;
        _logger = logger;
    }

    public async Task<CouponModel> GetDiscount(string productName)
    {
        var discountRequest = new GetDiscountRequest { ProductName = productName };

        try
        {
            var discount = await _discountProtoService.GetDiscountAsync(discountRequest);
            _logger.LogInformation("Discount is retrieved for Product : {productName}, Amount : {amount}", discount.ProductName, discount.Amount);
            return discount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not retrieve discount for product {ProductName}", productName);
            throw;
        }
    }
}
