using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Marten;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Persistence;

/// <summary>
/// Base repository implementation using Marten for PostgreSQL persistence
/// </summary>
public class MartenShoppingCartRepository : IShoppingCartRepository
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<MartenShoppingCartRepository> _logger;

    public MartenShoppingCartRepository(
        IDocumentStore documentStore,
        ILogger<MartenShoppingCartRepository> logger)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Getting basket for user {UserId} from PostgreSQL", userId);
            
            using var session = _documentStore.QuerySession();
            var cart = await session.LoadAsync<ShoppingCart>(userId);
            
            if (cart != null)
            {
                _logger.LogDebug("Found basket for user {UserId} with {ItemCount} items", 
                    userId, cart.Items.Count);
            }
            else
            {
                _logger.LogDebug("No basket found for user {UserId}", userId);
            }
            
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserId} from PostgreSQL", userId);
            throw;
        }
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        try
        {
            _logger.LogDebug("Storing basket for user {UserId} to PostgreSQL with {ItemCount} items", 
                cart.UserId, cart.Items.Count);
            
            using var session = _documentStore.LightweightSession();
            
            // Store the shopping cart
            session.Store(cart);
            await session.SaveChangesAsync();
            
            _logger.LogDebug("Successfully stored basket for user {UserId} to PostgreSQL", cart.UserId);
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing basket for user {UserId} to PostgreSQL", cart.UserId);
            throw;
        }
    }

    public async Task DeleteAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Deleting basket for user {UserId} from PostgreSQL", userId);
            
            using var session = _documentStore.LightweightSession();
            
            // Delete the shopping cart
            session.Delete<ShoppingCart>(userId);
            await session.SaveChangesAsync();
            
            _logger.LogDebug("Successfully deleted basket for user {UserId} from PostgreSQL", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user {UserId} from PostgreSQL", userId);
            throw;
        }
    }
}
