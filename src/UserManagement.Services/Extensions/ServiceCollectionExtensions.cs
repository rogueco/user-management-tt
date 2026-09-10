using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserManagement.Services.Services.ImportServices;
using UserManagement.Services.Services.LogServices;
using UserManagement.Services.Services.UserServices;

namespace UserManagement.Services.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton(TimeProvider.System);

        return serviceCollection
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUserLogService, UserLogService>()
            .AddScoped<IImportService, ImportService>()
            .AddScoped<ImportProcessor>();
    }

    // HybridCache is in-process on its own; with Redis configured it gains a shared second level.
    public static IServiceCollection AddCaching(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddHybridCache();

        if (configuration.GetConnectionString("Redis") is { Length: > 0 } redis)
        {
            serviceCollection.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis;
                options.InstanceName = "usermanagement:cache:";
            });
        }

        return serviceCollection;
    }
}
