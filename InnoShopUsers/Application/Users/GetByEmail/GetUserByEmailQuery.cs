using MediatR;

namespace Application.Users.GetByEmail
{
    public record GetUserByEmailQuery(string Email) : IRequest<UserResponse>;
    public record UserResponse(Guid Id, string Name, string Username, string Email);
}
