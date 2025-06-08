using System.Diagnostics;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using BasketAPI.Infrastructure.Metrics;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

public class MetricsShoppingCartRepositoryDecorator : IBasketRepository
{
    private readonly IBasketRepository _inner;
    private readonly IMetrics _metrics;

    public MetricsShoppingCartRepositoryDecorator(IBasketRepository inner, IMetrics metrics)
    {
        _inner = inner;
        _metrics = metrics;
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket)
    {
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var result = await _inner.StoreBasketAsync(basket);
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "store", "success");

            return result;
        }
        catch (Exception)
        {
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "store", "error");
            throw;
        }
        finally
        {
            timer.Stop();
            _metrics.ObserveHistogram(
                "basket_repository_operation_duration_seconds",
                "Duration of basket repository operations in seconds",
                timer.Elapsed.TotalSeconds,
                "store");
        }
    }

    public async Task DeleteBasketAsync(string userName)
    {
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            await _inner.DeleteBasketAsync(userName);
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "delete", "success");
        }
        catch (Exception)
        {
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "delete", "error");
            throw;
        }
        finally
        {
            timer.Stop();
            _metrics.ObserveHistogram(
                "basket_repository_operation_duration_seconds",
                "Duration of basket repository operations in seconds",
                timer.Elapsed.TotalSeconds,
                "delete");
        }
    }

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var result = await _inner.GetBasketAsync(userName);
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "get", result == null ? "miss" : "hit");

            return result;
        }
        finally
        {
            timer.Stop();
            _metrics.ObserveHistogram(
                "basket_repository_operation_duration_seconds",
                "Duration of basket repository operations in seconds",
                timer.Elapsed.TotalSeconds,
                "get");
        }
    }
}
