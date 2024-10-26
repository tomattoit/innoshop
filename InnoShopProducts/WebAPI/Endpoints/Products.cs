using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.Update;
using Application.Products.Get;
using Application.Products.GetAll;
using Domain.Entities;
using System.Security.Claims;
using Application.Products.Search;
using Application.Products.GetWithUser;

namespace WebAPI.Endpoints
{
    public class Products : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("products/create", async (HttpContext httpContext, CreateProductRequest request, ISender sender) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return Results.Unauthorized();
                }
                await sender.Send(new CreateProductCommand(request.Name, request.Description, request.Price, request.Quantity, Guid.Parse(userId)));
                return Results.Ok();
            }).RequireAuthorization();

            app.MapPost("products/search", async (SearchProductsQuery request, ISender sender) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(request));
                }
                catch (NoProductsException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapGet("products/{id:guid}", async (Guid id, ISender sender) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(new GetProductQuery(id)));
                }
                catch (ProductNotFoundException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapGet("/products/{id}/with-user", async (Guid id, IMediator mediator) =>
            {
                try
                {
                    return Results.Ok(await mediator.Send(new GetProductWithUserQuery(id)));
                }
                catch
                (ProductNotFoundException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapGet("products/getall", async (ISender sender) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(new GetAllProductsQuery()));
                }
                catch (NoProductsException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapPut("products/{id:guid}", async (HttpContext httpContext, Guid id, [FromBody] UpdateProductRequest request, ISender sender) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return Results.Unauthorized();
                }
                var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Quantity, Guid.Parse(userId));
                await sender.Send(command);
                return Results.NoContent();
            }).RequireAuthorization();

            app.MapDelete("products/{id:guid}", async (Guid id, HttpContext httpContext, ISender sender) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return Results.Unauthorized();
                }
                try 
                { 
                    await sender.Send(new DeleteProductCommand(id, Guid.Parse(userId)));
                    return Results.NoContent();
                }
                catch (ProductNotFoundException e)
                {
                    return Results.NotFound(e.Message);
                }
            }).RequireAuthorization();
        }
    }
}
