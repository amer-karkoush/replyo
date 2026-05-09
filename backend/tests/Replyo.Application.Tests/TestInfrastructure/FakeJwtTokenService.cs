using Replyo.Application.Common.Security;
using Replyo.Application.Common.Abstractions;
using Replyo.Domain.Entities;

namespace Replyo.Application.Tests.TestInfrastructure;

/// <summary>
/// Test double for <see cref="IJwtTokenService"/>. Returns deterministic tokens with
/// fixed expiries; the refresh token hash uses the same SHA-256 algorithm as the real
/// service so the handler's hash-on-lookup logic exercises the real path.
/// </summary>
/// <remarks>
/// Refresh token hashing is delegated to <see cref="RefreshTokenHasher"/> — the same
/// helper the production handler and real JwtTokenService use. This guarantees that
/// tokens issued by this fake are findable by the real handler under test.
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
        var refreshHash = RefreshTokenHasher.Hash(refreshPlaintext);

        return new IssuedTokens(
            AccessToken: $"fake-access-{user.Id}-{_issueCount}",
            AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshToken: refreshPlaintext,
            RefreshTokenHash: refreshHash,
            RefreshTokenExpiresAt: DateTimeOffset.UtcNow.AddDays(30));
    }


}