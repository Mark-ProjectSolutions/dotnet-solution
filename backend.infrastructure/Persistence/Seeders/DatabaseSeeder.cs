using backend.application.Common.Interfaces;
using backend.domain.Entities;
using backend.domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.infrastructure.Persistence.Seeders
{
    public sealed class DatabaseSeeder
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext db, IPasswordHasher hasher, ILogger<DatabaseSeeder> logger)
        {
            _db = db;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            await _db.Database.MigrateAsync(ct);

            if (await _db.Users.AnyAsync(ct))
            {
                _logger.LogInformation("Database already seeded — skipping.");
                return;
            }

            _logger.LogInformation("Seeding database...");

            var admin = User.Create(
                name: "Admin User",
                email: "admin@example.com",
                passwordHash: _hasher.Hash("Admin@1234!"),
                role: UserRole.Admin);

            var editor = User.Create(
                name: "Editor User",
                email: "editor@example.com",
                passwordHash: _hasher.Hash("Editor@1234!"),
                role: UserRole.Editor);

            await _db.Users.AddRangeAsync([admin, editor], ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Seeding complete. Admin: admin@example.com / Admin@1234!");
        }
    }
}
