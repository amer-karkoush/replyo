using Replyo.Domain.Common;

namespace Replyo.Domain.Entities;

public class RefreshToken : EntityBase
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;

    private RefreshToken() { }

    public static RefreshToken Issue(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? createdByIp = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));

        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp
        };
    }

    public void Revoke(string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Refresh token is already revoked.");

        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
        Touch();
    }
}