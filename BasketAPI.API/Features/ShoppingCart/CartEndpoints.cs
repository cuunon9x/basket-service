using BasketAPI.Application.Features.ShoppingCart.Commands;
using BasketAPI.Application.Features.ShoppingCart.Queries;
using BasketAPI.Domain.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace BasketAPI.API.Features.ShoppingCart;

/// <summary>
/// Endpoints for managing shopping cart operations
/// </summary>
public class CartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/basket")
            .WithTags("Basket")
            .RequireRateLimiting("fixed")
            .WithOpenApi(operation => new(operation)
            {
                Description = "Shopping cart operations for managing user baskets"
            });

        // Get basket by username
        group.MapGet("/{userName}", async ([FromRoute] string userName, [FromServices] ISender mediator) =>
        {
            var query = new GetBasketQuery(userName);
            try
            {
                var result = await mediator.Send(query);
                return result is null 
                    ? Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Basket not found",
                        detail: $"Basket for user {userName} was not found")
                    : Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error retrieving basket",
                    detail: ex.Message);
            }
        })
        .WithName("GetBasket")
        .Produces<Domain.Entities.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Update basket
        group.MapPost("/{userName}", async ([FromRoute] string userName, 
            [FromBody] UpdateBasketCommand command, [FromServices] ISender mediator) =>
        {
            try
            {
                if (userName != command.UserName)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Username mismatch",
                        detail: "The username in the URL does not match the username in the request body");
                }
                
                var result = await mediator.Send(command);
                return Results.Ok(result);
            }
            catch (ValidationException validationEx)
            {
                return Results.ValidationProblem(
                    validationEx.Errors.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value));
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error updating basket",
                    detail: ex.Message);
            }
        })
        .WithName("UpdateBasket")
        .Produces<Domain.Entities.ShoppingCart>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Delete basket
        group.MapDelete("/{userName}", async ([FromRoute] string userName, [FromServices] ISender mediator) =>
        {
            var command = new DeleteBasketCommand(userName);
            await mediator.Send(command);
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent);

        // Checkout basket
        group.MapPost("/checkout", async ([FromBody] CheckoutBasketCommand command, [FromServices] ISender mediator) =>
        {
            var result = await mediator.Send(command);
            return result ? Results.Accepted() : Results.BadRequest("Checkout failed");
        })
        .WithName("CheckoutBasket")
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
