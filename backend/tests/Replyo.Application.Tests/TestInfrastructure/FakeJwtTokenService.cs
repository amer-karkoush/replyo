using System.Security.Cryptography;
using System.Text;
using Replyo.Application.Common.Abstractions;
using Replyo.Domain.Entities;

namespace Replyo.Application.Tests.TestInfrastructure;

/// <summary>
/// Test double for <see cref="IJwtTokenService"/>. Returns deterministic tokens with
/// fixed expiries; the refresh token hash uses the same SHA-256 algorithm as the real
/// service so the handler's hash-on-lookup logic exercises the real path.
/// </summary>
/// <remarks>
/// Critical correctness point: the SHA-256 hashing here MUST match RefreshTokenHandler's
/// HashRefreshToken method. If the real JwtTokenService (Infrastructure, commit 4d) uses
/// a different algorithm, the hash duplication we deferred earlier becomes a real bug.
/// Tonight's PROGRESS should track this coupling.
/// </remarks>
internal sealed class FakeJwtTokenService : IJwtTokenService
{
    // Counter starts at 100 (not 0) so test seed code can use low suffixes (0, 1, 2...)
    // for "previously-issued" tokens without colliding with values this fake will produce.
    private int _issueCount = 100;

    public IssuedTokens Issue(User user, string? createdByIp)
    {
        _issueCount++;

        // Deterministic but distinct per call so rotation tests can tell tokens apart.
        var refreshPlaintext = $"fake-refresh-{user.Id}-{_issueCount}";
        var refreshHash = HashRefreshToken(refreshPlaintext);

        return new IssuedTokens(
            AccessToken: $"fake-access-{user.Id}-{_issueCount}",
            AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshToken: refreshPlaintext,
            RefreshTokenHash: refreshHash,
            RefreshTokenExpiresAt: DateTimeOffset.UtcNow.AddDays(30));
    }

    private static string HashRefreshToken(string plaintext)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}