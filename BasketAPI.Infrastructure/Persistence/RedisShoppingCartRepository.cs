using System.Text.Json;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Persistence;

public class RedisShoppingCartRepository : IShoppingCartRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisShoppingCartRepository> _logger;
    private const string KeyPrefix = "basket:";

    public RedisShoppingCartRepository(
        IConnectionMultiplexer redis,
        ILogger<RedisShoppingCartRepository> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var data = await db.StringGetAsync($"{KeyPrefix}{userId}");

            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<ShoppingCart>(data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shopping cart for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(cart);

            await db.StringSetAsync(
                $"{KeyPrefix}{cart.UserId}",
                serialized,
                TimeSpan.FromDays(30)); // Cache for 30 days

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shopping cart for user {UserId}", cart.UserId);
            throw;
        }
    }

    public async Task DeleteAsync(string userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{KeyPrefix}{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shopping cart for user {UserId}", userId);
            throw;
        }
    }
}
