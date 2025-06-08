using BasketAPI.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Application.Features.ShoppingCart.Queries;

public record GetBasketQuery(string UserName) : IRequest<Domain.Entities.ShoppingCart?>;

public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Domain.Entities.ShoppingCart?>
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<GetBasketQueryHandler> _logger;    public GetBasketQueryHandler(
        IBasketRepository repository,
        ILogger<GetBasketQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Domain.Entities.ShoppingCart?> Handle(
        GetBasketQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var basket = await _repository.GetBasketAsync(request.UserName);
            
            if (basket == null)
            {
                _logger.LogInformation("Basket not found for user {UserName}", request.UserName);
            }
            
            return basket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket for user {UserName}", request.UserName);
            throw;
        }
    }
}
