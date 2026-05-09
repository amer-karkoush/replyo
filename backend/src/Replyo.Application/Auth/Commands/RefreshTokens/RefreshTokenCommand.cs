namespace Replyo.Application.Auth.Commands.RefreshTokens;

/// <summary>
/// Refreshes an authentication session by exchanging a valid refresh token for a new
/// access + refresh token pair. The presented refresh token is revoked (rotation); a
/// new refresh token replaces it.
/// </summary>
/// <param name="RefreshToken">The plaintext refresh token previously issued. Hashed and looked up server-side.</param>
/// <param name="CreatedByIp">Request IP, recorded on the new refresh token. Null for non-HTTP entry points.</param>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? CreatedByIp);