using backend.application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.infrastructure.Identity
{
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; init; } = default!;
        public string Issuer { get; init; } = default!;
        public string Audience { get; init; } = default!;
        public int AccessTokenExpiryMinutes { get; init; } = 15;
        public int RefreshTokenExpiryDays { get; init; } = 7;
    }

    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        private ClaimsPrincipal? User => _http.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                var sub = User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(sub, out var id) ? id : null;
            }
        }

        public string? Role => User?.FindFirstValue(ClaimTypes.Role);
    }

}
