using MediatR;
using Domain.Entities;
using Application.Data;
using Application.Authentication;

namespace Application.Users.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        private IPasswordHasher _passwordHasher;
        public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }
        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User(Guid.NewGuid(), request.Name, request.Username, _passwordHasher.Hash(request.Password), request.Email);
            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
