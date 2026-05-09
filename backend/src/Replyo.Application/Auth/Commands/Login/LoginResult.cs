namespace Replyo.Application.Auth.Commands.Login;

/// <summary>
/// Result of a successful login: the authenticated user's IDs plus the access/refresh
/// token pair the client uses for subsequent requests.
/// </summary>
public sealed record LoginResult(
    Guid TenantId,
    Guid UserId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);