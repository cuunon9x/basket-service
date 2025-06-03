using BasketAPI.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BasketAPI.Application.Features.ShoppingCart.Commands;

public record DeleteBasketCommand(string UserName) : IRequest;

public class DeleteBasketCommandHandler : IRequestHandler<DeleteBasketCommand>
{
    private readonly IShoppingCartRepository _repository;
    private readonly ILogger<DeleteBasketCommandHandler> _logger;

    public DeleteBasketCommandHandler(
        IShoppingCartRepository repository,
        ILogger<DeleteBasketCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.DeleteAsync(request.UserName);
            _logger.LogInformation("Basket deleted for user {UserName}", request.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user {UserName}", request.UserName);
            throw;
        }
    }
}
