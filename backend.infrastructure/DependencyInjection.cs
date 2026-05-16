using backend.application.Common.Interfaces;
using backend.domain.Interfaces;
using backend.infrastructure.Identity;
using backend.infrastructure.Persistence;
using backend.infrastructure.Persistence.Repositories;
using backend.infrastructure.Persistence.Seeders;
using backend.infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            // ── Database ──────────────────────────────────────────────────────
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

            // ── Repositories ──────────────────────────────────────────────────
            services.AddScoped<IUserRepository, UserRepository>();

            // ── Seeders ───────────────────────────────────────────────────────
            services.AddScoped<DatabaseSeeder>();

            // ── Auth services ─────────────────────────────────────────────────
            services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // ── JWT settings ──────────────────────────────────────────────────
            var jwtSection = config.GetSection(JwtSettings.SectionName);
            services.Configure<JwtSettings>(jwtSection);

            var jwtSettings = jwtSection.Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings not configured.");

            // ── JWT authentication ─────────────────────────────────────────────
            services
                .AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero, // No grace period
                    };

                    opts.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            if (ctx.Exception is SecurityTokenExpiredException)
                                ctx.Response.Headers.Append("X-Token-Expired", "true");
                            return Task.CompletedTask;
                        },
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
                .AddPolicy("EditorPlus", p => p.RequireRole("Admin", "Editor"));

            return services;
        }
    }
}
