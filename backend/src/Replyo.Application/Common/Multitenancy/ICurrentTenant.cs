namespace Replyo.Application.Common.Multitenancy;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
    bool HasTenant => TenantId.HasValue && TenantId.Value != Guid.Empty;
}