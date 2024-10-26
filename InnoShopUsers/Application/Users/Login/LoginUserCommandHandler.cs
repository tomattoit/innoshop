using MediatR;
using Application.Authentication;
using Application.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Login
{
    internal sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private IApplicationDbContext _context;
        private IPasswordHasher _passwordHasher;
        private ITokenProvider _tokenProvider;
        public LoginUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
        }
        public async Task<string> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
            if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
            {
                throw new UserNotFoundByEmailException(command.Email);
            }
            return _tokenProvider.Create(user);
        }
    }
}
