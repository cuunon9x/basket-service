namespace BasketAPI.Infrastructure.Services.Grpc;

public interface IDiscountGrpcService
{
    Task<CouponModel> GetDiscount(string productName);
}
