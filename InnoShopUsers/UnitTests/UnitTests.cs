using Domain.Entities;
using Application.Users.Create;
using Application.Data;
using Moq;
using Application.Authentication;
using Application.Users.Delete;
using Application.Users.Get;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Application.Users.GetByEmail;
using Application.Users.Login;
using Application.Users.Update;

namespace UnitTests
{
    public class HandlersTests
    {
        private IApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var mockPublisher = new Mock<IPublisher>();
            var context = new ApplicationDbContext(options, mockPublisher.Object);
            context.Database.EnsureCreated();

            return context;
        }
        [Fact]
        public async Task Handle_ShouldCreateUser()
        {
            var name = "John Doe";
            var email = "johndoe@gmail.com";
            var password = "password";
            var username = "johndoe";
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var handler = new CreateUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object, mockPasswordHasher.Object);
            await handler.Handle(new CreateUserCommand(name, username, password, email), CancellationToken.None);
            mockUserRepository.Verify(repo => repo.Add(It.Is<User>(u => 
            u.Name == name && 
            u.Email == email && 
            u.Username == username && 
            u.PasswordHash == mockPasswordHasher.Object.Hash(password))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRemoveUser_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var user = new User(userId, "Test User", "test", "12345", "test@gmail.com");
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var handler = new DeleteUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object);
            await handler.Handle(new DeleteUserCommand(userId), CancellationToken.None);

            mockUserRepository.Verify(repo => repo.Remove(user), Times.Once);
            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_GetByIdAsync()
        {
            var userId = Guid.NewGuid();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            var handler = new DeleteUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object);

            await Assert.ThrowsAsync<UserNotFoundException>(() => handler.Handle(new DeleteUserCommand(userId), CancellationToken.None));

            mockUserRepository.Verify(repo => repo.Remove(It.IsAny<User>()), Times.Never);
            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserResponse_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var context = CreateDbContext();

            var testUser = new User
            (
                userId,
                "Test User",
                "testuser",
                "hashedpassword",
                "test@example.com"
            );
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            var handler = new GetUserQueryHandler(context);

            var result = await handler.Handle(new GetUserQuery(userId), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var context = CreateDbContext();

            var handler = new GetUserQueryHandler(context);

            await Assert.ThrowsAsync<UserNotFoundException>(() => handler.Handle(new GetUserQuery(userId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnUserResponse_WhenUserWithEmailExists()
        {
            var userEmail = "test@example.com";
            var context = CreateDbContext();

            var testUser = new User
            (
                Guid.NewGuid(),
                "Test User",
                "testuser",
                "hashedpassword",
                userEmail
            );
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            var handler = new GetUserByEmailQueryHandler(context);

            var result = await handler.Handle(new GetUserByEmailQuery(userEmail), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(testUser.Id, result.Id);
            Assert.Equal(testUser.Name, result.Name);
            Assert.Equal(testUser.Username, result.Username);
            Assert.Equal(testUser.Email, result.Email);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundByEmailException_GetByEmail()
        {
            var userEmail = "notfound@example.com";
            var context = CreateDbContext();

            var handler = new GetUserByEmailQueryHandler(context);

            await Assert.ThrowsAsync<UserNotFoundByEmailException>(() => handler.Handle(new GetUserByEmailQuery(userEmail), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var userEmail = "test@example.com";
            var userPassword = "password123";
            var hashedPassword = "hashedpassword";
            var context = CreateDbContext();

            var testUser = new User
            (
                Guid.NewGuid(),
                "Test User",
                "testuser",
                hashedPassword,
                userEmail
            );
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            mockPasswordHasher
                .Setup(hasher => hasher.Verify(userPassword, hashedPassword))
                .Returns(true);

            var mockTokenProvider = new Mock<ITokenProvider>();
            mockTokenProvider
                .Setup(provider => provider.Create(It.Is<User>(u => u.Id == testUser.Id)))
                .Returns("test-token");

            var handler = new LoginUserCommandHandler(context, mockPasswordHasher.Object, mockTokenProvider.Object);

            var result = await handler.Handle(new LoginUserCommand(userEmail, userPassword), CancellationToken.None);

            Assert.Equal("test-token", result);

            mockPasswordHasher.Verify(hasher => hasher.Verify(userPassword, hashedPassword), Times.Once);
            mockTokenProvider.Verify(provider => provider.Create(It.Is<User>(u => u.Id == testUser.Id)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundByEmailException_WhenUserWithEmailDoesNotExist()
        {
            var userEmail = "notfound@example.com";
            var userPassword = "password123";
            var context = CreateDbContext();

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockTokenProvider = new Mock<ITokenProvider>();

            var handler = new LoginUserCommandHandler(context, mockPasswordHasher.Object, mockTokenProvider.Object);

            await Assert.ThrowsAsync<UserNotFoundByEmailException>(() => handler.Handle(new LoginUserCommand(userEmail, userPassword), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundByEmailException_WhenPasswordIsIncorrect()
        {
            var userEmail = "test@example.com";
            var incorrectPassword = "incorrectPassword";
            var hashedPassword = "hashedpassword";
            var context = CreateDbContext();

            var testUser = new User
            (
                Guid.NewGuid(),
                "Test User",
                "testuser",
                hashedPassword,
                userEmail
            );
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            mockPasswordHasher.Setup(hasher => hasher.Verify(incorrectPassword, hashedPassword)).Returns(false);

            var mockTokenProvider = new Mock<ITokenProvider>();

            var handler = new LoginUserCommandHandler(context, mockPasswordHasher.Object, mockTokenProvider.Object);

            await Assert.ThrowsAsync<UserNotFoundByEmailException>(() => handler.Handle(new LoginUserCommand(userEmail, incorrectPassword), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateUser_WhenUserExistsAndDataIsValid()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User
            (
                userId,
                "Old Name",
                "oldusername",
                "oldhashedpassword",
                "old@example.com"
            );

            var updatedName = "New Name";
            var updatedUsername = "newusername";
            var updatedEmail = "new@example.com";
            var newPassword = "newpassword";
            var newHashedPassword = "newhashedpassword";

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            mockPasswordHasher.Setup(hasher => hasher.Hash(newPassword))
                .Returns(newHashedPassword);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            var handler = new UpdateUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object, mockPasswordHasher.Object);

            var command = new UpdateUserCommand
            (
                userId,
                updatedName,
                updatedUsername,
                newPassword,
                updatedEmail
            );

            await handler.Handle(command, CancellationToken.None);

            mockUserRepository.Verify(repo => repo.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.Name == updatedName &&
                u.Username == updatedUsername &&
                u.Email == updatedEmail &&
                u.PasswordHash == newHashedPassword
            )), Times.Once);

            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenUserDoesNotExist_Update()
        {
            var userId = Guid.NewGuid();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var handler = new UpdateUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object, mockPasswordHasher.Object);

            var command = new UpdateUserCommand
            (
                userId,
                "New Name",
                "newusername",
                "new@example.com",
                "newpassword"
            );

            await Assert.ThrowsAsync<UserNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldNotUpdatePasswordHash_WhenPasswordIsNotProvided()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User
            (
                userId,
                "Old Name",
                "oldusername",
                "oldhashedpassword",
                "old@example.com"
            );

            var updatedName = "New Name";
            var updatedUsername = "newusername";
            var updatedEmail = "new@example.com";

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var mockPasswordHasher = new Mock<IPasswordHasher>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            var handler = new UpdateUserCommandHandler(mockUserRepository.Object, mockUnitOfWork.Object, mockPasswordHasher.Object);

            var command = new UpdateUserCommand
            (
                userId,
                updatedName,
                updatedUsername,
                null,
                updatedEmail
            );

            await handler.Handle(command, CancellationToken.None);

            mockUserRepository.Verify(repo => repo.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.Name == updatedName &&
                u.Username == updatedUsername &&
                u.Email == updatedEmail &&
                u.PasswordHash == "oldhashedpassword" // Пароль не изменился
            )), Times.Once);

            mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}