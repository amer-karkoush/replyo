using System.ComponentModel.DataAnnotations;

namespace Replyo.Application.Common.Configuration;

/// <summary>
/// Strongly-typed configuration for JWT issuance and validation. Bound from the
/// <c>Jwt</c> configuration section in <c>appsettings.json</c> via
/// <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name to bind from.</summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// The HS256 signing key. Must be at least 32 bytes (256 bits) of entropy when
    /// UTF-8 encoded; shorter keys will fail key length validation at startup.
    /// </summary>
    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>The expected <c>iss</c> claim on issued tokens.</summary>
    [Required]
    public string Issuer { get; init; } = string.Empty;

    /// <summary>The expected <c>aud</c> claim on issued tokens.</summary>
    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Lifetime of issued access tokens. Short by design — refresh tokens carry the
    /// long-lived session.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "01:00:00")]
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Lifetime of issued refresh tokens. Long-lived; rotated on every refresh; revocable.
    /// </summary>
    [Range(typeof(TimeSpan), "1.00:00:00", "90.00:00:00")]
    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);
}