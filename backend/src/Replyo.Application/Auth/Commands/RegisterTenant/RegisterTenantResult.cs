namespace Replyo.Application.Auth.Commands.RegisterTenant;

/// <summary>
/// Result of a successful registration: the new tenant + Owner user IDs and the
/// access/refresh token pair the client uses for subsequent requests.
/// </summary>
public sealed record RegisterTenantResult(
    Guid TenantId,
    Guid UserId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);