using System;
using System.Collections.Generic;
using System.Text;

namespace backend.domain.Errors
{
    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);

        // ── Auth errors ───────────────────────────────────────────────────────
        public static readonly Error InvalidCredentials =
            new("Auth.InvalidCredentials", "Email or password is incorrect.");

        public static readonly Error InvalidRefreshToken =
            new("Auth.InvalidRefreshToken", "Refresh token is invalid or expired.");

        public static readonly Error AccountInactive =
            new("Auth.AccountInactive", "This account has been deactivated.");

        // ── User errors ───────────────────────────────────────────────────────
        public static readonly Error UserNotFound =
            new("User.NotFound", "User was not found.");

        public static readonly Error EmailAlreadyExists =
            new("User.EmailAlreadyExists", "A user with this email already exists.");

        public static readonly Error Unauthorised =
            new("User.Unauthorised", "You are not authorised to perform this action.");

        // ── Validation fallback ───────────────────────────────────────────────
        public static Error Validation(string message) =>
            new("Validation.Failed", message);
    }
}
