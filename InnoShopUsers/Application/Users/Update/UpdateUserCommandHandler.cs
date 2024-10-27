using MediatR;
using Application.Data;
using Domain.Entities;
using Application.Authentication;

namespace Application.Users.Update
{
    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        private IPasswordHasher _passwordHasher;
        public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }
        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
            {
                throw new UserNotFoundException(request.Id);
            }
            user.Update(request.Name, request.Username, !string.IsNullOrEmpty(request.Password) ? _passwordHasher.Hash(request.Password) : user.PasswordHash, request.Email);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
