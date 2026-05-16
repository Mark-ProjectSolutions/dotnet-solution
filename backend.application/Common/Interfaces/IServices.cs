using backend.domain.Entities;

namespace backend.application.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        /// <summary>Returns the userId from an expired (but otherwise valid) access token.</summary>
        Guid? GetUserIdFromExpiredToken(string token);
    }

    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
        string? Role { get; }
    }
}
