using backend.application.Auth.DTOs;
using backend.application.Common.Interfaces;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using MediatR;

namespace backend.application.Auth.Queries.GetCurrentUser
{
    public sealed record GetCurrentUserQuery : IRequest<Result<UserDto>>;

    public sealed class GetCurrentUserQueryHandler
        : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _currentUser;

        public GetCurrentUserQueryHandler(IUserRepository users, ICurrentUserService currentUser)
        {
            _users = users;
            _currentUser = currentUser;
        }

        public async Task<Result<UserDto>> Handle(GetCurrentUserQuery _, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<UserDto>(Error.Unauthorised);

            var user = await _users.GetByIdAsync(_currentUser.UserId.Value, ct);
            if (user is null)
                return Result.Failure<UserDto>(Error.UserNotFound);

            return Result.Success(new UserDto(
                user.Id, user.Name, user.Email,
                user.Role.ToString(), user.AvatarUrl, user.CreatedAt));
        }
    }
}
