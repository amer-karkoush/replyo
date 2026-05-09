namespace Replyo.Application.Auth.Commands.RefreshTokens;

/// <summary>
/// Result of a successful token refresh: a new access + refresh token pair. The
/// previously-presented refresh token is now revoked; clients must use the new
/// pair on subsequent requests.
/// </summary>
public sealed record RefreshTokenResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);