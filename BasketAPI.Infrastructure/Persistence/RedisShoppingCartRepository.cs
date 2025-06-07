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
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromDays(30);

    public RedisShoppingCartRepository(
        IConnectionMultiplexer redis,
        ILogger<RedisShoppingCartRepository> logger)
    {
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static string GetKey(string userId) => $"{KeyPrefix}{userId}"; public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var data = await db.StringGetAsync(GetKey(userId));

            if (data.IsNullOrEmpty)
            {
                _logger.LogInformation("Shopping cart not found for user {UserId}", userId);
                return null;
            }

            var cart = JsonSerializer.Deserialize<ShoppingCart>(data!, _jsonOptions);
            _logger.LogInformation("Retrieved shopping cart for user {UserId} with {ItemCount} items",
                userId, cart?.Items.Count ?? 0);
            return cart;
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
            var serialized = JsonSerializer.Serialize(cart, _jsonOptions);

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
