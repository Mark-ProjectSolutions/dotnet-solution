using backend.application.Common.Interfaces;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace backend.application.Users.Commands.DeleteUser
{
    public sealed record DeleteUserCommand(Guid Id) : IRequest<Result>;

    public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }

    public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public DeleteUserCommandHandler(
            IUserRepository users, ICurrentUserService currentUser, IUnitOfWork uow)
        {
            _users = users;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
        {
            if (_currentUser.Role != "Admin")
                return Result.Failure(Error.Unauthorised);

            var user = await _users.GetByIdAsync(cmd.Id, ct);
            if (user is null)
                return Result.Failure(Error.UserNotFound);

            // Soft-delete: deactivate rather than removing from DB
            user.Deactivate();
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
