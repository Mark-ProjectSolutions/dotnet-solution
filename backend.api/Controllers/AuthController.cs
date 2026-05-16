using backend.api.Extensions;
using backend.application.Auth.Commands.Login;
using backend.application.Auth.Commands.RefreshToken;
using backend.application.Auth.Commands.Register;
using backend.application.Auth.DTOs;
using backend.application.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender) => _sender = sender;

        /// <summary>Register a new user account.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command,
            CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            return result.ToActionResult();
        }

        /// <summary>Sign in with email and password.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            return result.ToActionResult();
        }

        /// <summary>Refresh an expired access token using a refresh token.</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenCommand command,
            CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            return result.ToActionResult();
        }

        /// <summary>Get the currently authenticated user's profile.</summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var result = await _sender.Send(new GetCurrentUserQuery(), ct);
            return result.ToActionResult();
        }
    }

}
