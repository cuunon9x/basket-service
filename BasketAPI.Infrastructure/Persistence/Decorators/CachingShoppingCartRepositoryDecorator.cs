using System.Text.Json;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

/// <summary>
/// Caching decorator that adds Redis caching on top of the base repository
/// </summary>
public class CachingShoppingCartRepositoryDecorator : IBasketRepository
{
    private readonly IBasketRepository _baseRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachingShoppingCartRepositoryDecorator> _logger;
    private const string KeyPrefix = "basket:";
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24); // 24 hours cache

    public CachingShoppingCartRepositoryDecorator(
        IBasketRepository baseRepository,
        IConnectionMultiplexer redis,
        ILogger<CachingShoppingCartRepositoryDecorator> logger)
    {
        _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static string GetKey(string userName) => $"{KeyPrefix}{userName}";

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        try
        {
            var key = GetKey(userName);
            var db = _redis.GetDatabase();

            // Try to get from cache first
            var cachedData = await db.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache HIT: Retrieved basket for user {UserName} from Redis", userName);
                return JsonSerializer.Deserialize<ShoppingCart>(cachedData!, _jsonOptions);
            }

            _logger.LogDebug("Cache MISS: Getting basket for user {UserName} from PostgreSQL", userName);

            // Get from base repository (PostgreSQL)
            var cart = await _baseRepository.GetBasketAsync(userName);

            // Cache the result if found
            if (cart != null)
            {
                var serializedCart = JsonSerializer.Serialize(cart, _jsonOptions);
                await db.StringSetAsync(key, serializedCart, _cacheExpiry);
                _logger.LogDebug("Cached basket for user {UserName} in Redis", userName);
            }

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while getting basket for user {UserName}", userName);
            // If caching fails, fallback to base repository
            return await _baseRepository.GetBasketAsync(userName);
        }
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart cart)
    {
        try
        {
            // Update in base repository (PostgreSQL) first
            var updatedCart = await _baseRepository.StoreBasketAsync(cart);

            // Update cache
            var key = GetKey(cart.UserName);
            var db = _redis.GetDatabase();
            var serializedCart = JsonSerializer.Serialize(updatedCart, _jsonOptions);
            await db.StringSetAsync(key, serializedCart, _cacheExpiry);

            _logger.LogDebug("Updated basket for user {UserName} in both PostgreSQL and Redis cache", cart.UserName);
            return updatedCart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while updating basket for user {UserName}", cart.UserName);

            // Try to invalidate cache if update failed
            try
            {
                var key = GetKey(cart.UserName);
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Invalidated cache for user {UserName} due to update error", cart.UserName);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Failed to invalidate cache for user {UserName}", cart.UserName);
            }

            throw;
        }
    }

    public async Task DeleteBasketAsync(string userName)
    {
        try
        {
            // Delete from base repository (PostgreSQL) first
            await _baseRepository.DeleteBasketAsync(userName);

            // Remove from cache
            var key = GetKey(userName);
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            _logger.LogDebug("Deleted basket for user {UserName} from both PostgreSQL and Redis cache", userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while deleting basket for user {UserName}", userName);

            // Try to invalidate cache even if delete failed
            try
            {
                var key = GetKey(userName);
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Invalidated cache for user {UserName} due to delete error", userName);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Failed to invalidate cache for user {UserName}", userName);
            }

            throw;
        }
    }
}
