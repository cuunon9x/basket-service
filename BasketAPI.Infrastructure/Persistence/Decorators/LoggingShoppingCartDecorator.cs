using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

public class LoggingShoppingCartDecorator : IBasketRepository
{
    private readonly IBasketRepository _decorated;
    private readonly ILogger<LoggingShoppingCartDecorator> _logger;

    public LoggingShoppingCartDecorator(
        IBasketRepository decorated,
        ILogger<LoggingShoppingCartDecorator> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Getting basket for user {UserName}", userName);
            var result = await _decorated.GetBasketAsync(userName);
            stopwatch.Stop();

            _logger.LogInformation(
                "Got basket for user {UserName} in {ElapsedMilliseconds}ms. Found: {Found}",
                userName, stopwatch.ElapsedMilliseconds, result != null);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserName}", userName);
            throw;
        }
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart cart)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation(
                "Storing basket for user {UserName}. Items count: {ItemsCount}",
                cart.UserName, cart.Items.Count);

            var result = await _decorated.StoreBasketAsync(cart);
            stopwatch.Stop();

            _logger.LogInformation(
                "Stored basket for user {UserName} in {ElapsedMilliseconds}ms",
                cart.UserName, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing basket for user {UserName}", cart.UserName);
            throw;
        }
    }

    public async Task DeleteBasketAsync(string userName)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Deleting basket for user {UserName}", userName);
            await _decorated.DeleteBasketAsync(userName);
            stopwatch.Stop();

            _logger.LogInformation(
                "Deleted basket for user {UserName} in {ElapsedMilliseconds}ms",
                userName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user {UserName}", userName);
            throw;
        }
    }
}
