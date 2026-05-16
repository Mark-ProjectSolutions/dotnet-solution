using backend.domain.Entities;
using backend.domain.Enum;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.tests.Users
{
    public sealed class UserEntityTests
    {
        [Fact]
        public void Create_ValidArgs_CreatesUser()
        {
            var user = User.Create("Alice", "alice@example.com", "hash");

            user.Name.Should().Be("Alice");
            user.Email.Should().Be("alice@example.com");
            user.Role.Should().Be(UserRole.Viewer);
            user.IsActive.Should().BeTrue();
            user.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Create_NormalisesEmailToLower()
        {
            var user = User.Create("Bob", "Bob@EXAMPLE.COM", "hash");
            user.Email.Should().Be("bob@example.com");
        }

        [Fact]
        public void Create_EmptyName_Throws()
        {
            var act = () => User.Create("", "test@test.com", "hash");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var user = User.Create("Test", "test@test.com", "hash");
            user.Deactivate();
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void HasValidRefreshToken_WithMatchingHashAndFutureExpiry_ReturnsTrue()
        {
            var user = User.Create("Test", "test@test.com", "hash");
            user.SetRefreshToken("token-hash", DateTime.UtcNow.AddDays(1));

            user.HasValidRefreshToken("token-hash").Should().BeTrue();
        }

        [Fact]
        public void HasValidRefreshToken_WithExpiredToken_ReturnsFalse()
        {
            var user = User.Create("Test", "test@test.com", "hash");
            user.SetRefreshToken("token-hash", DateTime.UtcNow.AddDays(-1));

            user.HasValidRefreshToken("token-hash").Should().BeFalse();
        }
    }
}
