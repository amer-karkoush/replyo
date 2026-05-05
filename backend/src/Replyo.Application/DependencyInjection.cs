using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Replyo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        // Handlers will be registered here in commit 4b.
        return services;
    }
}