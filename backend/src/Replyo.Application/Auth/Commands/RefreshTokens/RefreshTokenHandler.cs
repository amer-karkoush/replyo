using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Replyo.Application.Common.Abstractions;
using Replyo.Application.Common.Exceptions;
using Replyo.Application.Common.Security;
using Replyo.Domain.Entities;

namespace Replyo.Application.Auth.Commands.RefreshTokens;

/// <summary>
/// Handles refresh token rotation with replay detection. On a valid presentation the old
/// token is revoked and a new pair is issued. On replay (presentation of an already-revoked
/// token) all of the user's active refresh tokens are revoked — the legitimate user has to
/// log in fresh, which is the correct response since we can't distinguish legitimate from
/// malicious in this case.
/// </summary>
public interface IRefreshTokenHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>;

internal sealed class RefreshTokenHandler : IRefreshTokenHandler
{
    private readonly IValidator<RefreshTokenCommand> _validator;
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _tokenService;

    public RefreshTokenHandler(
        IValidator<RefreshTokenCommand> validator,
        IApplicationDbContext db,
        IJwtTokenService tokenService)
    {
        _validator = validator;
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<RefreshTokenResult> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Hash the presented token with the same algorithm the issuer used (SHA-256).
        // We never store or compare plaintext refresh tokens.
        var presentedHash = RefreshTokenHasher.Hash(command.RefreshToken);

        // Include the user so we can re-issue without a second roundtrip.
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == presentedHash, cancellationToken);

        if (stored is null)
            throw new UnauthorizedException("Refresh token is invalid.");

        // Replay detection. If the presented token is already revoked, this is either a
        // legitimate user replaying an old token (benign) or an attacker presenting a
        // token whose chain has rotated past it (malicious). We can't tell which, so we
        // assume the worst and revoke all active tokens for this user — log them out
        // everywhere. The legitimate user logs in fresh; the attacker's chain dies too.
        if (stored.IsRevoked)
        {
            await RevokeAllActiveTokensAsync(stored.UserId, command.CreatedByIp, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token replay detected. All sessions revoked.");
        }

        if (stored.IsExpired)
            throw new UnauthorizedException("Refresh token has expired.");

        if (!stored.User.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        // Happy path: rotate. Issue a new pair, revoke the old token pointing at the new one.
        var newTokens = _tokenService.Issue(stored.User, command.CreatedByIp);

        stored.Revoke(
            revokedByIp: command.CreatedByIp,
            replacedByTokenHash: newTokens.RefreshTokenHash);

        var newRefreshToken = RefreshToken.Issue(
            stored.UserId,
            newTokens.RefreshTokenHash,
            newTokens.RefreshTokenExpiresAt,
            command.CreatedByIp);

        _db.RefreshTokens.Add(newRefreshToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(
            newTokens.AccessToken,
            newTokens.AccessTokenExpiresAt,
            newTokens.RefreshToken,
            newTokens.RefreshTokenExpiresAt);
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        string? revokedByIp,
        CancellationToken ct)
    {
        // Load every still-active token for this user and revoke them in-memory.
        // The change tracker picks up the mutations; SaveChangesAsync flushes them.
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.Revoke(revokedByIp);
    }

}