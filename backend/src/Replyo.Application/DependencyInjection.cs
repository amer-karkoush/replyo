using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Replyo.Application.Auth.Commands.Login;
using Replyo.Application.Auth.Commands.RefreshTokens;
using Replyo.Application.Auth.Commands.RegisterTenant;

namespace Replyo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        services.AddScoped<IRegisterTenantHandler, RegisterTenantHandler>();
        services.AddScoped<ILoginHandler, LoginHandler>();
        services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();
        return services;
    }
}