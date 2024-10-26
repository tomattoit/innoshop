using MediatR;

namespace Application.Users.Create
{
    public record CreateUserCommand(string Name, string Username, string Password, string Email) : IRequest;
}
