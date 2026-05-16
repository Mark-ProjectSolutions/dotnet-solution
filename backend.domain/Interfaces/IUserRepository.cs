using backend.domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<(IReadOnlyList<User> Items, int Total)> GetPagedAsync(
            int page, int limit, string? search, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Update(User user);
        void Delete(User user);
        Task<bool> ExistsAsync(string email, CancellationToken ct = default);
    }
}
