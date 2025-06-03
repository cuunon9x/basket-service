using BasketAPI.Application.Features.ShoppingCart.Commands;
using BasketAPI.Application.Features.ShoppingCart.Queries;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BasketAPI.API.Features.ShoppingCart;

public class CartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/basket").WithTags("Basket");

        // Get basket by username
        group.MapGet("/{userName}", async (string userName, ISender mediator) =>
        {
            var query = new GetBasketQuery(userName);
            var result = await mediator.Send(query);
            
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetBasket")
        .Produces<Domain.Entities.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Update basket
        group.MapPost("/{userName}", async (string userName, 
            UpdateBasketCommand command, ISender mediator) =>
        {
            if (userName != command.UserName)
                return Results.BadRequest("Username mismatch");
                
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateBasket")
        .Produces<Domain.Entities.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Delete basket
        group.MapDelete("/{userName}", async (string userName, ISender mediator) =>
        {
            var command = new DeleteBasketCommand(userName);
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent);

        // Checkout basket
        group.MapPost("/checkout", async (CheckoutBasketCommand command, ISender mediator) =>
        {
            var result = await mediator.Send(command);
            return result ? Results.Accepted() : Results.BadRequest("Checkout failed");
        })
        .WithName("CheckoutBasket")
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
