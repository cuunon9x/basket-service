using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

public class CachingShoppingCartDecorator : IShoppingCartRepository
{
    private readonly IShoppingCartRepository _decorated;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachingShoppingCartDecorator> _logger;
    private const string KeyPrefix = "basket:cache:";
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);
    private readonly JsonSerializerOptions _jsonOptions;

    public CachingShoppingCartDecorator(
        IShoppingCartRepository decorated,
        IConnectionMultiplexer redis,
        ILogger<CachingShoppingCartDecorator> logger)
    {
        _decorated = decorated;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        var key = GetKey(userId);
        var db = _redis.GetDatabase();

        // Try to get from cache first
        var cachedValue = await db.StringGetAsync(key);
        if (!cachedValue.IsNull)
        {
            _logger.LogInformation("Cache hit for basket {UserId}", userId);
            return JsonSerializer.Deserialize<ShoppingCart>(cachedValue!, _jsonOptions);
        }

        // If not in cache, get from repository
        _logger.LogInformation("Cache miss for basket {UserId}", userId);
        var cart = await _decorated.GetByUserIdAsync(userId);
        
        // Cache the result if not null
        if (cart != null)
        {
            await CacheBasketAsync(cart);
        }

        return cart;
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        // Update in repository first
        var updated = await _decorated.UpdateAsync(cart);
        
        // Then update cache
        await CacheBasketAsync(updated);
        
        return updated;
    }

    public async Task DeleteAsync(string userId)
    {
        // Delete from repository first
        await _decorated.DeleteAsync(userId);
        
        // Then remove from cache
        var key = GetKey(userId);
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        
        _logger.LogInformation("Deleted basket from cache for {UserId}", userId);
    }

    private async Task CacheBasketAsync(ShoppingCart cart)
    {
        var key = GetKey(cart.UserId);
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(cart, _jsonOptions);
        
        await db.StringSetAsync(key, serialized, _cacheExpiry);
        _logger.LogInformation("Updated cache for basket {UserId}", cart.UserId);
    }

    private static string GetKey(string userId) => $"{KeyPrefix}{userId}";
}
