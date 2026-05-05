using Replyo.Domain.Entities;

namespace Replyo.Application.Common.Abstractions;

/// <summary>
/// Issues access tokens and refresh tokens for authenticated users. Access tokens are
/// stateless JWTs signed with a symmetric key (HS256); refresh tokens are opaque
/// random strings whose hashes are persisted via <see cref="Domain.Entities.RefreshToken"/>.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Issues a new access + refresh token pair for the supplied user. Persisting the
    /// refresh token's hash to the database is the caller's responsibility — this method
    /// returns the values but does not touch storage.
    /// </summary>
    /// <param name="user">The authenticated user. Must have <see cref="User.TenantId"/>, <see cref="User.Id"/>, and <see cref="User.Role"/> populated.</param>
    /// <param name="createdByIp">The IP address that initiated the token issuance, recorded on the refresh token for audit. May be null for non-HTTP entry points.</param>
    /// <returns>An <see cref="IssuedTokens"/> bundle containing both tokens and the access token's expiry.</returns>
    IssuedTokens Issue(User user, string? createdByIp);
}

/// <summary>
/// A freshly-issued access + refresh token pair, plus the metadata callers need to
/// persist the refresh token and return the access token to the client.
/// </summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="AccessTokenExpiresAt">The instant at which the access token expires.</param>
/// <param name="RefreshToken">The plaintext refresh token. Returned to the client; never persisted in plaintext.</param>
/// <param name="RefreshTokenHash">The SHA-256 hash of the refresh token. Persisted to the database for later lookup.</param>
/// <param name="RefreshTokenExpiresAt">The instant at which the refresh token expires.</param>
public sealed record IssuedTokens(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAt);