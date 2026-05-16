using System;
using System.Collections.Generic;
using System.Text;

namespace backend.application.Users.DTOs
{
    public sealed record UserSummaryDto(
        Guid Id,
        string Name,
        string Email,
        string Role,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? LastLoginAt);
}
