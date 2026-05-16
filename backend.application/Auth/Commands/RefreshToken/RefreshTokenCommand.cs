using backend.application.Auth.DTOs;
using backend.application.Common.Interfaces;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace backend.application.Auth.Commands.RefreshToken
{
    // ── Command ───────────────────────────────────────────────────────────────
    public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken)
        : IRequest<Result<AuthResponse>>;

    // ── Validator ─────────────────────────────────────────────────────────────
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    // ── Handler ───────────────────────────────────────────────────────────────
    public sealed class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IUnitOfWork _uow;

        public RefreshTokenCommandHandler(
            IUserRepository users,
            IPasswordHasher hasher,
            ITokenService tokens,
            IUnitOfWork uow)
        {
            _users = users;
            _hasher = hasher;
            _tokens = tokens;
            _uow = uow;
        }

        public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
        {
            // Extract userId from the expired (but structurally valid) access token
            var userId = _tokens.GetUserIdFromExpiredToken(cmd.AccessToken);
            if (userId is null)
                return Result.Failure<AuthResponse>(Error.InvalidRefreshToken);

            var user = await _users.GetByIdAsync(userId.Value, ct);
            if (user is null || !user.IsActive)
                return Result.Failure<AuthResponse>(Error.InvalidRefreshToken);

            // Verify the refresh token hash — same approach as password verification
            var providedHash = _hasher.Hash(cmd.RefreshToken);
            if (!user.HasValidRefreshToken(providedHash))
                return Result.Failure<AuthResponse>(Error.InvalidRefreshToken);

            // Rotate — issue new pair, invalidate old one
            var newAccessToken = _tokens.GenerateAccessToken(user);
            var newRefreshToken = _tokens.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            user.SetRefreshToken(_hasher.Hash(newRefreshToken), refreshExpiry);
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(new AuthResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(15),
                User: new UserDto(
                    user.Id, user.Name, user.Email,
                    user.Role.ToString(), user.AvatarUrl, user.CreatedAt)));
        }
    }
}
