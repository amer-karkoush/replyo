using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Replyo.Application.Common.Abstractions;
using Replyo.Application.Common.Multitenancy;
using Replyo.Infrastructure.Persistence;

namespace Replyo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string not configured.");

        services.AddDbContext<ReplyoDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector());

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ReplyoDbContext>());

        // Temporary placeholder until JWT bearer middleware reads tenant_id from claims.
        services.AddScoped<ICurrentTenant, NoTenantCurrentTenant>();

        return services;
    }
}

internal sealed class NoTenantCurrentTenant : ICurrentTenant
{
    public Guid? TenantId => null;
}