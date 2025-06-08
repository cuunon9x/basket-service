using BasketAPI.Domain.Common;
using BasketAPI.Domain.Entities;
using Marten;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Infrastructure.Persistence;

/// <summary>
/// Base repository implementation using Marten for PostgreSQL persistence
/// </summary>
public class MartenShoppingCartRepository : IBasketRepository
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

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        try
        {
            _logger.LogDebug("Getting basket for user {UserName} from PostgreSQL", userName);

            using var session = _documentStore.QuerySession();
            var cart = await session.LoadAsync<ShoppingCart>(userName);

            if (cart != null)
            {
                _logger.LogDebug("Found basket for user {UserName} with {ItemCount} items",
                    userName, cart.Items.Count);
            }
            else
            {
                _logger.LogDebug("No basket found for user {UserName}", userName);
            }

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserName} from PostgreSQL", userName);
            throw;
        }
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart cart)
    {
        try
        {
            _logger.LogDebug("Storing basket for user {UserName} to PostgreSQL with {ItemCount} items",
                cart.UserName, cart.Items.Count);

            using var session = _documentStore.LightweightSession();

            // Store the shopping cart
            session.Store(cart);
            await session.SaveChangesAsync();

            _logger.LogDebug("Successfully stored basket for user {UserName} to PostgreSQL", cart.UserName);
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing basket for user {UserName} to PostgreSQL", cart.UserName);
            throw;
        }
    }

    public async Task DeleteBasketAsync(string userName)
    {
        try
        {
            _logger.LogDebug("Deleting basket for user {UserName} from PostgreSQL", userName);

            using var session = _documentStore.LightweightSession();

            // Delete the shopping cart
            session.Delete<ShoppingCart>(userName);
            await session.SaveChangesAsync();

            _logger.LogDebug("Successfully deleted basket for user {UserName} from PostgreSQL", userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user {UserName} from PostgreSQL", userName);
            throw;
        }
    }
}
