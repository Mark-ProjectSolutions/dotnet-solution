using backend.domain.Entities;
using backend.domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Users.FindAsync([id], ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<(IReadOnlyList<User> Items, int Total)> GetPagedAsync(
            int page, int limit, string? search, CancellationToken ct = default)
        {
            var query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
            => await _db.Users.AddAsync(user, ct);

        public void Update(User user) => _db.Users.Update(user);

        public void Delete(User user) => _db.Users.Remove(user);

        public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
            => await _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
    }
}
