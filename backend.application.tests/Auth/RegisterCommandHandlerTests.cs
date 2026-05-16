using backend.application.Auth.Commands.Register;
using backend.application.Common.Interfaces;
using backend.domain.Entities;
using backend.domain.Errors;
using backend.domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.tests.Auth
{
    public sealed class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _usersRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly Mock<ITokenService> _tokens = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private RegisterCommandHandler CreateHandler() => new(
            _usersRepo.Object, _hasher.Object, _tokens.Object, _uow.Object);

        [Fact]
        public async Task Handle_NewEmail_CreatesUserAndReturnsTokens()
        {
            _usersRepo.Setup(r => r.ExistsAsync("new@example.com", default)).ReturnsAsync(false);
            _usersRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
            _tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
            _tokens.Setup(t => t.GenerateRefreshToken()).Returns("refresh");
            _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await CreateHandler().Handle(
                new RegisterCommand("New User", "new@example.com", "Password1!"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.User.Name.Should().Be("New User");
            result.Value.User.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task Handle_ExistingEmail_ReturnsEmailAlreadyExists()
        {
            _usersRepo.Setup(r => r.ExistsAsync("taken@example.com", default)).ReturnsAsync(true);

            var result = await CreateHandler().Handle(
                new RegisterCommand("User", "taken@example.com", "Password1!"), default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Error.EmailAlreadyExists);
        }
    }
}
