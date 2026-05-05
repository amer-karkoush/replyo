namespace Replyo.Application.Auth.Commands.RegisterTenant;

/// <summary>
/// Registers a new tenant and its initial Owner user atomically.
/// </summary>
/// <param name="TenantName">Display name for the tenant. Used to derive the slug.</param>
/// <param name="OwnerEmail">Email for the initial Owner user. Must be globally unique.</param>
/// <param name="OwnerPassword">Plaintext password. Hashed before persistence; never stored or logged.</param>
/// <param name="OwnerFullName">Full name of the Owner user.</param>
/// <param name="CreatedByIp">Request IP, recorded on the issued refresh token. Null for non-HTTP entry points.</param>
public sealed record RegisterTenantCommand(
    string TenantName,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFullName,
    string? CreatedByIp);