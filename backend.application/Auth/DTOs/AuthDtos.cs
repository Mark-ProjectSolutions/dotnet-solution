using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.Auth.DTOs
{
    public sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt,
        UserDto User);

    public sealed record UserDto(
        Guid Id,
        string Name,
        string Email,
        string Role,
        string? AvatarUrl,
        DateTime CreatedAt);
}
