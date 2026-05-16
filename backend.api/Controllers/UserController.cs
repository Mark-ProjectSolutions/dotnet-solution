using backend.api.Extensions;
using backend.application.Common.Models;
using backend.application.Users.Commands.DeleteUser;
using backend.application.Users.Commands.UpdateUser;
using backend.application.Users.DTOs;
using backend.application.Users.Queries.GetUserById;
using backend.application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [Produces("application/json")]
    public sealed class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender) => _sender = sender;

        /// <summary>List users with pagination and optional search.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(PaginatedList<UserSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var result = await _sender.Send(new GetUsersQuery(page, limit, search), ct);
            return result.ToActionResult();
        }

        /// <summary>Get a single user by ID.</summary>
        [HttpGet("{id:guid}", Name = "GetUserById")]
        [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new GetUserByIdQuery(id), ct);
            return result.ToActionResult();
        }

        /// <summary>Update a user's profile (own profile or admin-only for others).</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            var result = await _sender.Send(
                new UpdateUserCommand(id, request.Name, request.AvatarUrl), ct);
            return result.ToActionResult();
        }

        /// <summary>Deactivate (soft-delete) a user. Admin only.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeleteUserCommand(id), ct);
            return result.ToActionResult();
        }
    }

    // Request body record (kept in controller file for colocation)
    public sealed record UpdateUserRequest(string Name, string? AvatarUrl);

}
