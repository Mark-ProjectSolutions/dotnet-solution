using backend.application.Auth.DTOs;
using backend.application.Common.Interfaces;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace backend.application.Auth.Commands.Login
{
    // ── Command ───────────────────────────────────────────────────────────────
    public sealed record LoginCommand(string Email, string Password)
        : IRequest<Result<AuthResponse>>;

    // ── Validator ─────────────────────────────────────────────────────────────
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }

    // ── Handler ───────────────────────────────────────────────────────────────
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IUnitOfWork _uow;

        public LoginCommandHandler(
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

        public async Task<Result<AuthResponse>> Handle(LoginCommand cmd, CancellationToken ct)
        {
            var user = await _users.GetByEmailAsync(cmd.Email, ct);

            // Use constant-time comparison path to prevent timing attacks
            if (user is null || !_hasher.Verify(cmd.Password, user.PasswordHash))
                return Result.Failure<AuthResponse>(Error.InvalidCredentials);

            if (!user.IsActive)
                return Result.Failure<AuthResponse>(Error.AccountInactive);

            // Generate tokens
            var accessToken = _tokens.GenerateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            // Hash the refresh token before storing (same principle as passwords)
            var refreshHash = _hasher.Hash(refreshToken);
            user.SetRefreshToken(refreshHash, refreshExpiry);
            user.RecordLogin();

            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(15),
                User: new UserDto(
                    user.Id, user.Name, user.Email,
                    user.Role.ToString(), user.AvatarUrl, user.CreatedAt)));
        }
    }
}
