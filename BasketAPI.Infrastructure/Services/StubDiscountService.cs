using BasketAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Services;

/// <summary>
/// Stub implementation of IDiscountService for when the Discount service is not available.
/// Returns zero discount for all products.
/// </summary>
public class StubDiscountService : IDiscountService
{
    private readonly ILogger<StubDiscountService> _logger;

    public StubDiscountService(ILogger<StubDiscountService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<(decimal Amount, string Description)> GetDiscountAsync(string productName)
    {
        _logger.LogInformation("Stub discount service called for product {ProductName}. Returning no discount.", productName);
        return Task.FromResult((0m, "No discount service available"));
    }
}
