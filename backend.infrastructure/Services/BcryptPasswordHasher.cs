using backend.application.Common.Interfaces;
using BCryptLib = BCrypt.Net.BCrypt;

namespace backend.infrastructure.Services
{
    public sealed class BcryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            return BCryptLib.HashPassword(password, WorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            // BCrypt.Verify is constant-time — safe against timing attacks
            return BCryptLib.Verify(password, hash);
        }
    }
}
