using System.Diagnostics;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using BasketAPI.Infrastructure.Metrics;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

public class MetricsShoppingCartRepositoryDecorator : IShoppingCartRepository
{
    private readonly IShoppingCartRepository _inner;
    private readonly IMetrics _metrics;

    public MetricsShoppingCartRepositoryDecorator(IShoppingCartRepository inner, IMetrics metrics)
    {
        _inner = inner;
        _metrics = metrics;
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart basket)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        try
        {
            var result = await _inner.UpdateAsync(basket);
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "update", "success");
            
            return result;
        }
        catch (Exception)
        {
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "update", "error");
            throw;
        }
        finally
        {
            timer.Stop();
            _metrics.ObserveHistogram(
                "basket_repository_operation_duration_seconds",
                "Duration of basket repository operations in seconds",
                timer.Elapsed.TotalSeconds,
                "update");
        }
    }

    public async Task DeleteAsync(string userName)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        try
        {
            await _inner.DeleteAsync(userName);
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

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var result = await _inner.GetByUserIdAsync(userId);
            _metrics.IncrementCounter(
                "basket_repository_operations_total",
                "Total number of basket repository operations",
                "getbyuserid", result == null ? "miss" : "hit");

            return result;
        }
        finally
        {
            timer.Stop();
            _metrics.ObserveHistogram(
                "basket_repository_operation_duration_seconds",
                "Duration of basket repository operations in seconds",
                timer.Elapsed.TotalSeconds,
                "getbyuserid");
        }
    }
}
