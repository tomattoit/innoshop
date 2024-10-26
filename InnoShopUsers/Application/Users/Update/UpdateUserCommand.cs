using Domain.Entities;
using MediatR;

namespace Application.Users.Update
{
    public record UpdateUserCommand(Guid Id, string Name, string Username, string Password, string Email) : IRequest;
    public record UpdateUserRequest(string Name, string Username, string Password, string Email);
}
