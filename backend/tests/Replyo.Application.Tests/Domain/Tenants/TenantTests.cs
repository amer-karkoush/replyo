using FluentAssertions;
using Replyo.Domain.Entities;
using Replyo.Domain.Enums;
using Xunit;

namespace Replyo.Application.Tests.Domain.Tenants;

public class TenantTests
{
    [Fact]
    public void Create_WithValidInputs_SetsExpectedFields()
    {
        // Arrange
        const string name = "Acme Corp";
        const string slug = "acme-corp";

        // Act
        var tenant = Tenant.Create(name, slug);

        // Assert
        tenant.Should().NotBeNull();
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be(name);
        tenant.Slug.Should().Be(slug);
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}