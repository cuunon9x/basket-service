using System.Text.Json;
using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BasketAPI.Infrastructure.Persistence.Decorators;

/// <summary>
/// Caching decorator that adds Redis caching on top of the base repository
/// </summary>
public class CachingShoppingCartRepositoryDecorator : IShoppingCartRepository
{
    private readonly IShoppingCartRepository _baseRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachingShoppingCartRepositoryDecorator> _logger;
    private const string KeyPrefix = "basket:";
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24); // 24 hours cache

    public CachingShoppingCartRepositoryDecorator(
        IShoppingCartRepository baseRepository,
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

    private static string GetKey(string userId) => $"{KeyPrefix}{userId}";

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        try
        {
            var key = GetKey(userId);
            var db = _redis.GetDatabase();
            
            // Try to get from cache first
            var cachedData = await db.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache HIT: Retrieved basket for user {UserId} from Redis", userId);
                return JsonSerializer.Deserialize<ShoppingCart>(cachedData!, _jsonOptions);
            }

            _logger.LogDebug("Cache MISS: Getting basket for user {UserId} from PostgreSQL", userId);
            
            // Get from base repository (PostgreSQL)
            var cart = await _baseRepository.GetByUserIdAsync(userId);
            
            // Cache the result if found
            if (cart != null)
            {
                var serializedCart = JsonSerializer.Serialize(cart, _jsonOptions);
                await db.StringSetAsync(key, serializedCart, _cacheExpiry);
                _logger.LogDebug("Cached basket for user {UserId} in Redis", userId);
            }
            
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while getting basket for user {UserId}", userId);
            // If caching fails, fallback to base repository
            return await _baseRepository.GetByUserIdAsync(userId);
        }
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        try
        {
            // Update in base repository (PostgreSQL) first
            var updatedCart = await _baseRepository.UpdateAsync(cart);
            
            // Update cache
            var key = GetKey(cart.UserId);
            var db = _redis.GetDatabase();
            var serializedCart = JsonSerializer.Serialize(updatedCart, _jsonOptions);
            await db.StringSetAsync(key, serializedCart, _cacheExpiry);
            
            _logger.LogDebug("Updated basket for user {UserId} in both PostgreSQL and Redis cache", cart.UserId);
            return updatedCart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while updating basket for user {UserId}", cart.UserId);
            
            // Try to invalidate cache if update failed
            try
            {
                var key = GetKey(cart.UserId);
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Invalidated cache for user {UserId} due to update error", cart.UserId);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Failed to invalidate cache for user {UserId}", cart.UserId);
            }
            
            throw;
        }
    }

    public async Task DeleteAsync(string userId)
    {
        try
        {
            // Delete from base repository (PostgreSQL) first
            await _baseRepository.DeleteAsync(userId);
            
            // Remove from cache
            var key = GetKey(userId);
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
            
            _logger.LogDebug("Deleted basket for user {UserId} from both PostgreSQL and Redis cache", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching decorator while deleting basket for user {UserId}", userId);
            
            // Try to invalidate cache even if delete failed
            try
            {
                var key = GetKey(userId);
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogDebug("Invalidated cache for user {UserId} due to delete error", userId);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Failed to invalidate cache for user {UserId}", userId);
            }
            
            throw;
        }
    }
}
