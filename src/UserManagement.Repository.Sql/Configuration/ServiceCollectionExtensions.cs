using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Repository.Sql.Repositories;
using UserManagement.Repository.Sql.Seeding;
using UserManagement.Services.Repositories;

namespace UserManagement.Repository.Sql.Configuration;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> options)
    {
        serviceCollection.AddDbContext<UserManagementDbContext>(options);

        return serviceCollection
            .AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UserManagementDbContext>())
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserLogRepository, UserLogRepository>()
            .AddScoped<IImportJobRepository, ImportJobRepository>()
            .AddScoped<DatabaseSeeder>();
    }

    // EnableDynamicJson lets Npgsql serialise the change lists straight into jsonb columns.
    public static DbContextOptionsBuilder UseUserManagementPostgres(this DbContextOptionsBuilder options, string connectionString)
        => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .EnableRetryOnFailure()
                .ConfigureDataSource(dataSource => dataSource.EnableDynamicJson()))
            .UseSnakeCaseNamingConvention();

    public static async Task InitialiseDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(cancellationToken);
    }
}
