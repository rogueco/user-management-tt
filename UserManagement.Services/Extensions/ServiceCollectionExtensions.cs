using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        return services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUserLogService, UserLogService>();
    }
}
