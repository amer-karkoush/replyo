using Replyo.Application.Common.Multitenancy;

namespace Replyo.Application.Tests.TestInfrastructure;

/// <summary>
/// Test double for <see cref="ICurrentTenant"/>. Defaults to no current tenant —
/// matches the production auth flow, where registration and login both run
/// pre-authentication. Set <see cref="TenantId"/> on the instance for tests of
/// post-auth handlers (Week 2+).
/// </summary>
internal sealed class FakeCurrentTenant : ICurrentTenant
{
    public Guid? TenantId { get; set; }
}