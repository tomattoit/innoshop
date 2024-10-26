using MediatR;

namespace Application.Users.Login
{
    public sealed record class LoginUserCommand(string Email, string Password) : IRequest<string>;
}
