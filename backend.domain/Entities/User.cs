using backend.domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.domain.Entities
{
    public sealed class User : BaseEntity
    {
        // ── Fields persisted to DB ────────────────────────────────────────────
        public string Name { get; private set; } = default!;
        public string Email { get; private set; } = default!;

        /// <summary>BCrypt hash of the password. Never store plaintext.</summary>
        public string PasswordHash { get; private set; } = default!;

        public UserRole Role { get; private set; } = UserRole.Viewer;
        public string? AvatarUrl { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime? LastLoginAt { get; private set; }

        // ── Refresh token (stored hashed) ─────────────────────────────────────
        public string? RefreshTokenHash { get; private set; }
        public DateTime? RefreshTokenExpiresAt { get; private set; }

        // ── EF Core requires a parameterless constructor ───────────────────────
        private User() { }

        // ── Factory method — enforce invariants at creation ────────────────────
        public static User Create(string name, string email, string passwordHash, UserRole role = UserRole.Viewer)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

            return new User
            {
                Name = name.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                Role = role,
            };
        }

        // ── Behaviour methods ─────────────────────────────────────────────────
        public void UpdateProfile(string name, string? avatarUrl = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
            AvatarUrl = avatarUrl;
            Touch();
        }

        public void ChangeRole(UserRole role)
        {
            Role = role;
            Touch();
        }

        public void SetPasswordHash(string hash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(hash);
            PasswordHash = hash;
            Touch();
        }

        public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

        public void Deactivate()
        {
            IsActive = false;
            Touch();
        }

        public void SetRefreshToken(string tokenHash, DateTime expiresAt)
        {
            RefreshTokenHash = tokenHash;
            RefreshTokenExpiresAt = expiresAt;
            Touch();
        }

        public void RevokeRefreshToken()
        {
            RefreshTokenHash = null;
            RefreshTokenExpiresAt = null;
            Touch();
        }

        public bool HasValidRefreshToken(string tokenHash) =>
            RefreshTokenHash == tokenHash &&
            RefreshTokenExpiresAt.HasValue &&
            RefreshTokenExpiresAt.Value > DateTime.UtcNow;
    }
}
