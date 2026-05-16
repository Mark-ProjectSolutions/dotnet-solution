using backend.application.Common.Interfaces;
using backend.application.Users.DTOs;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace backend.application.Users.Commands.UpdateUser
{
    public sealed record UpdateUserCommand(Guid Id, string Name, string? AvatarUrl)
        : IRequest<Result<UserSummaryDto>>;

    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AvatarUrl).MaximumLength(500).When(x => x.AvatarUrl is not null);
        }
    }

    public sealed class UpdateUserCommandHandler
        : IRequestHandler<UpdateUserCommand, Result<UserSummaryDto>>
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public UpdateUserCommandHandler(
            IUserRepository users,
            ICurrentUserService currentUser,
            IUnitOfWork uow)
        {
            _users = users;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task<Result<UserSummaryDto>> Handle(UpdateUserCommand cmd, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(cmd.Id, ct);
            if (user is null)
                return Result.Failure<UserSummaryDto>(Error.UserNotFound);

            // Only admins or the user themselves can update the profile
            var isAdmin = _currentUser.Role == "Admin";
            if (!isAdmin && _currentUser.UserId != cmd.Id)
                return Result.Failure<UserSummaryDto>(Error.Unauthorised);

            user.UpdateProfile(cmd.Name, cmd.AvatarUrl);
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(new UserSummaryDto(
                user.Id, user.Name, user.Email, user.Role.ToString(),
                user.IsActive, user.CreatedAt, user.LastLoginAt));
        }
    }

}
