using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

public class LoggingShoppingCartDecorator : IShoppingCartRepository
{
    private readonly IShoppingCartRepository _decorated;
    private readonly ILogger<LoggingShoppingCartDecorator> _logger;

    public LoggingShoppingCartDecorator(
        IShoppingCartRepository decorated,
        ILogger<LoggingShoppingCartDecorator> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Getting basket for user {UserId}", userId);
            var result = await _decorated.GetByUserIdAsync(userId);
            stopwatch.Stop();

            _logger.LogInformation(
                "Got basket for user {UserId} in {ElapsedMilliseconds}ms. Found: {Found}",
                userId, stopwatch.ElapsedMilliseconds, result != null);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation(
                "Updating basket for user {UserId}. Items count: {ItemsCount}",
                cart.UserId, cart.Items.Count);

            var result = await _decorated.UpdateAsync(cart);
            stopwatch.Stop();

            _logger.LogInformation(
                "Updated basket for user {UserId} in {ElapsedMilliseconds}ms",
                cart.UserId, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket for user {UserId}", cart.UserId);
            throw;
        }
    }

    public async Task DeleteAsync(string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Deleting basket for user {UserId}", userId);
            await _decorated.DeleteAsync(userId);
            stopwatch.Stop();

            _logger.LogInformation(
                "Deleted basket for user {UserId} in {ElapsedMilliseconds}ms",
                userId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user {UserId}", userId);
            throw;
        }
    }
}
