using MediatR;
using Domain.Entities;

namespace Application.Users.Get
{
    public record GetUserQuery(Guid Id) : IRequest<UserResponse>;
    public record UserResponse(Guid Id, string Name, string Username, string Email);
}
