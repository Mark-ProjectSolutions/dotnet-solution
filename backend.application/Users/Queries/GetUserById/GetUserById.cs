using backend.application.Users.DTOs;
using backend.domain.Errors;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserSummaryDto>>;

    public sealed class GetUserByIdQueryHandler
        : IRequestHandler<GetUserByIdQuery, Result<UserSummaryDto>>
    {
        private readonly IUserRepository _users;

        public GetUserByIdQueryHandler(IUserRepository users) => _users = users;

        public async Task<Result<UserSummaryDto>> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(query.Id, ct);
            if (user is null)
                return Result.Failure<UserSummaryDto>(Error.UserNotFound);

            return Result.Success(new UserSummaryDto(
                user.Id, user.Name, user.Email, user.Role.ToString(),
                user.IsActive, user.CreatedAt, user.LastLoginAt));
        }
    }
}
