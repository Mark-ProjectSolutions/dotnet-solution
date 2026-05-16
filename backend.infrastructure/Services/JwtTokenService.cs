using backend.application.Common.Interfaces;
using backend.domain.Entities;
using backend.infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace backend.infrastructure.Services
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly JwtSecurityTokenHandler _handler = new();

        public JwtTokenService(IOptions<JwtSettings> settings)
            => _settings = settings.Value;

        // ── Access token — short-lived (15 min default) ────────────────────────
        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name,  user.Name),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
                signingCredentials: credentials);

            return _handler.WriteToken(token);
        }

        // ── Refresh token — cryptographically random, long-lived ──────────────
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        // ── Extract userId from an expired access token ────────────────────────
        public Guid? GetUserIdFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.SecretKey)),
                ValidateLifetime = false, // ← allow expired tokens here
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                var principal = _handler.ValidateToken(token, parameters, out _);
                var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return Guid.TryParse(sub, out var id) ? id : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
