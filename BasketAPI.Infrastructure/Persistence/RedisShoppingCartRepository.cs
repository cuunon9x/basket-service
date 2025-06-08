using System.Text.Json;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Persistence;

public class RedisShoppingCartRepository : IBasketRepository
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

    private static string GetKey(string userName) => $"{KeyPrefix}{userName}";    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        try
        {
            var db = _redis.GetDatabase();
            var data = await db.StringGetAsync(GetKey(userName));

            if (data.IsNullOrEmpty)
            {
                _logger.LogInformation("Shopping cart not found for user {UserName}", userName);
                return null;
            }

            var cart = JsonSerializer.Deserialize<ShoppingCart>(data!, _jsonOptions);
            _logger.LogInformation("Retrieved shopping cart for user {UserName} with {ItemCount} items",
                userName, cart?.Items.Count ?? 0);
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shopping cart for user {UserName}", userName);
            throw;
        }
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart cart)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(cart, _jsonOptions);

            await db.StringSetAsync(
                $"{KeyPrefix}{cart.UserName}",
                serialized,
                TimeSpan.FromDays(30)); // Cache for 30 days

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shopping cart for user {UserName}", cart.UserName);
            throw;
        }
    }

    public async Task DeleteBasketAsync(string userName)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{KeyPrefix}{userName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shopping cart for user {UserName}", userName);
            throw;
        }
    }
}
