using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserManagement.API.Options;

namespace UserManagement.API.Configuration;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        serviceCollection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        serviceCollection.AddSwaggerGen();

        return serviceCollection;
    }

    public static IServiceCollection AddPlatformHealthChecks(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var healthChecks = serviceCollection.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Default")!, name: "postgres", tags: ["ready"]);

        if (configuration.GetConnectionString("Redis") is { Length: > 0 } redis)
        {
            healthChecks.AddRedis(redis, name: "redis", tags: ["ready"]);
        }

        return serviceCollection;
    }
}
