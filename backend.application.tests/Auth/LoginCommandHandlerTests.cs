using backend.application.Auth.Commands.Login;
using backend.application.Common.Interfaces;
using backend.domain.Entities;
using backend.domain.Enum;
using backend.domain.Errors;
using backend.domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.tests.Auth
{
    public sealed class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _usersRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly Mock<ITokenService> _tokens = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private LoginCommandHandler CreateHandler() => new(
            _usersRepo.Object, _hasher.Object, _tokens.Object, _uow.Object);

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var user = User.Create("Test User", "test@example.com", "hashed", UserRole.Viewer);

            _usersRepo.Setup(r => r.GetByEmailAsync("test@example.com", default))
                      .ReturnsAsync(user);
            _hasher.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("refresh-hash");
            _tokens.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
            _tokens.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
            _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

            var handler = CreateHandler();
            var command = new LoginCommand("test@example.com", "Password1!");

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access-token");
            result.Value.RefreshToken.Should().Be("refresh-token");
            result.Value.User.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsInvalidCredentials()
        {
            _usersRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
                      .ReturnsAsync((User?)null);

            var result = await CreateHandler().Handle(
                new LoginCommand("nobody@example.com", "Password1!"), default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Error.InvalidCredentials);
        }

        [Fact]
        public async Task Handle_WrongPassword_ReturnsInvalidCredentials()
        {
            var user = User.Create("Test", "test@example.com", "hashed");
            _usersRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
            _hasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

            var result = await CreateHandler().Handle(
                new LoginCommand("test@example.com", "wrong"), default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Error.InvalidCredentials);
        }

        [Fact]
        public async Task Handle_InactiveUser_ReturnsAccountInactive()
        {
            var user = User.Create("Test", "test@example.com", "hashed");
            user.Deactivate();

            _usersRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
            _hasher.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);

            var result = await CreateHandler().Handle(
                new LoginCommand("test@example.com", "Password1!"), default);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(Error.AccountInactive);
        }
    }

}
