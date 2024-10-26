using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Users.Create;
using Application.Users.Delete;
using Application.Users.Update;
using Application.Users.Get;
using Domain.Entities;
using Application.Users.Login;
using Application.Users.GetByEmail;
using System.Security.Claims;

namespace WebAPI.Endpoints
{
    public class Users : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("users", async (CreateUserCommand command, ISender sender) =>
            {
                await sender.Send(command);
                return Results.Ok();
            });

            app.MapGet("users/{email}", async (string email, ISender sender) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(new GetUserByEmailQuery(email)));
                }
                catch (UserNotFoundByEmailException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapGet("users/{id:guid}", async (Guid id, ISender sender) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(new GetUserQuery(id)));
                }
                catch (UserNotFoundException e)
                {
                    return Results.NotFound(e.Message);
                }
            });

            app.MapPut("users/me", async (HttpContext httpContext, [FromBody] UpdateUserRequest request, ISender sender) =>
            {
                var id = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (id is null)
                {
                    return Results.Unauthorized();
                }
                var command = new UpdateUserCommand(Guid.Parse(id), request.Name, request.Username, request.Password, request.Email);
                await sender.Send(command);
                return Results.NoContent();
            }).RequireAuthorization();

            app.MapDelete("users/{id:guid}", async (Guid id, ISender sender) =>
            {
                try 
                { 
                    await sender.Send(new DeleteUserCommand(id));
                    return Results.NoContent();
                }
                catch (UserNotFoundException e)
                {
                    return Results.NotFound(e.Message);
                }
            });
            app.MapPost("users/login", async (LoginUserCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                try
                {
                    return Results.Ok(await sender.Send(command));
                }
                catch (UserNotFoundByEmailException e)
                {
                    return Results.NotFound(e.Message);
                }
            });
        }
    }
}
