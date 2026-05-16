using backend.application.Common.Models;
using backend.application.Users.DTOs;
using backend.domain.Interfaces;
using backend.domain.ValueObjects;
using MediatR;

namespace backend.application.Users.Queries.GetUsers
{
    public sealed record GetUsersQuery(int Page = 1, int Limit = 20, string? Search = null)
        : IRequest<Result<PaginatedList<UserSummaryDto>>>;

    public sealed class GetUsersQueryHandler
        : IRequestHandler<GetUsersQuery, Result<PaginatedList<UserSummaryDto>>>
    {
        private readonly IUserRepository _users;

        public GetUsersQueryHandler(IUserRepository users) => _users = users;

        public async Task<Result<PaginatedList<UserSummaryDto>>> Handle(
            GetUsersQuery query, CancellationToken ct)
        {
            var (items, total) = await _users.GetPagedAsync(
                query.Page, query.Limit, query.Search, ct);

            var dtos = items.Select(u => new UserSummaryDto(
                u.Id, u.Name, u.Email, u.Role.ToString(),
                u.IsActive, u.CreatedAt, u.LastLoginAt)).ToList();

            return Result.Success(new PaginatedList<UserSummaryDto>(
                dtos, total, query.Page, query.Limit));
        }
    }
}
