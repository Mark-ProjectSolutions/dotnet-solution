using backend.application.Auth.DTOs;
using backend.domain.Errors;
using backend.domain.Entities;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using FluentValidation;
using MediatR;
using backend.application.Common.Interfaces;

namespace backend.application.Auth.Commands.Register
{
    // ── Command ───────────────────────────────────────────────────────────────
    public sealed record RegisterCommand(string Name, string Email, string Password)
        : IRequest<Result<AuthResponse>>;

    // ── Validator ─────────────────────────────────────────────────────────────
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(72).WithMessage("Password must not exceed 72 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }

    // ── Handler ───────────────────────────────────────────────────────────────
    public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IUnitOfWork _uow;

        public RegisterCommandHandler(
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

        public async Task<Result<AuthResponse>> Handle(RegisterCommand cmd, CancellationToken ct)
        {
            if (await _users.ExistsAsync(cmd.Email, ct))
                return Result.Failure<AuthResponse>(Error.EmailAlreadyExists);

            var hash = _hasher.Hash(cmd.Password);
            var user = User.Create(cmd.Name, cmd.Email, hash);

            // Generate tokens immediately so the user is logged in after registration
            var accessToken = _tokens.GenerateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            user.SetRefreshToken(_hasher.Hash(refreshToken), refreshExpiry);

            await _users.AddAsync(user, ct);
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
