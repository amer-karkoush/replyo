using Replyo.Domain.Common;
using Replyo.Domain.Enums;

namespace Replyo.Domain.Entities;

public class User : EntityBase
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastLoginAt { get; private set; }

    private User() { }

    public static User CreateOwner(Guid tenantId, string email, string passwordHash, string fullName)
        => Create(tenantId, email, passwordHash, fullName, UserRole.Owner);

    public static User CreateMember(Guid tenantId, string email, string passwordHash, string fullName)
        => Create(tenantId, email, passwordHash, fullName, UserRole.Member);

    private static User Create(Guid tenantId, string email, string passwordHash, string fullName, UserRole role)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        return new User
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        Touch();
    }
}