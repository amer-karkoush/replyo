using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Replyo.Application.Common;
using Replyo.Application.Common.Abstractions;
using Replyo.Application.Common.Configuration;
using Replyo.Application.Common.Exceptions;
using Replyo.Domain.Entities;

namespace Replyo.Application.Auth.Commands.RegisterTenant;

/// <summary>
/// Handles tenant registration: creates the tenant, the Owner user, and an initial refresh
/// token in a single database transaction.
/// </summary>
public interface IRegisterTenantHandler : ICommandHandler<RegisterTenantCommand, RegisterTenantResult>;

internal sealed class RegisterTenantHandler : IRegisterTenantHandler
{
    private readonly IValidator<RegisterTenantCommand> _validator;
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public RegisterTenantHandler(
        IValidator<RegisterTenantCommand> validator,
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService)
    {
        _validator = validator;
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<RegisterTenantResult> HandleAsync(
        RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Normalize once — Domain.User.Create lowercases internally, so the uniqueness
        // probe must use the same form to avoid case-mismatched duplicates.
        var normalizedEmail = command.OwnerEmail.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
            throw new ConflictException($"A user with email '{normalizedEmail}' already exists.");

        var slug = await GenerateUniqueSlugAsync(command.TenantName, cancellationToken);

        var passwordHash = _passwordHasher.Hash(command.OwnerPassword);

        var tenant = Tenant.Create(command.TenantName, slug);
        var owner = User.CreateOwner(tenant.Id, normalizedEmail, passwordHash, command.OwnerFullName);

        var tokens = _tokenService.Issue(owner, command.CreatedByIp);
        var refreshToken = RefreshToken.Issue(
            owner.Id,
            tokens.RefreshTokenHash,
            tokens.RefreshTokenExpiresAt,
            command.CreatedByIp);

        _db.Tenants.Add(tenant);
        _db.Users.Add(owner);
        _db.RefreshTokens.Add(refreshToken);

        // Single SaveChangesAsync wraps all three inserts in one implicit transaction.
        // If any insert fails (unique constraint race, etc.), nothing is persisted.
        await _db.SaveChangesAsync(cancellationToken);

        return new RegisterTenantResult(
            tenant.Id,
            owner.Id,
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt);
    }

    private async Task<string> GenerateUniqueSlugAsync(string tenantName, CancellationToken ct)
    {
        var baseSlug = SlugGenerator.Generate(tenantName);

        // Edge case: tenant name had no slug-able characters (e.g., all emoji).
        // Fall back to a random slug so registration doesn't hard-fail on weird names.
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "tenant";

        if (!await _db.Tenants.AnyAsync(t => t.Slug == baseSlug, ct))
            return baseSlug;

        // Collision — append a 6-char random suffix. Loop in case of repeated collision,
        // bounded so we never spin forever; in practice the first attempt resolves it.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
            if (!await _db.Tenants.AnyAsync(t => t.Slug == candidate, ct))
                return candidate;
        }

        throw new InvalidOperationException(
            $"Could not generate a unique slug for '{tenantName}' after 5 attempts.");
    }
}