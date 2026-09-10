using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Messaging.Consumers;
using UserManagement.Messaging.Publishers;
using UserManagement.Repository.Sql;
using UserManagement.Services.Messaging;

namespace UserManagement.Messaging.Configuration;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    // RabbitMQ when configured, otherwise an in-process bus. Either way messages go through the
    // transactional outbox, so a job and its trigger message commit together or not at all.
    public static IServiceCollection AddMessaging(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection("RabbitMq");
        var useRabbit = rabbit["Host"] is { Length: > 0 };
        var hostConsumers = configuration.GetValue<bool?>("Messaging:HostConsumers") ?? !useRabbit;

        serviceCollection.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        serviceCollection.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<UserManagementDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
            });

            if (hostConsumers)
            {
                bus.AddConsumer<ImportRequestedConsumer>();
                bus.AddConfigureEndpointsCallback((context, _, endpoint) => endpoint.UseEntityFrameworkOutbox<UserManagementDbContext>(context));
            }

            if (useRabbit)
            {
                bus.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", host =>
                    {
                        host.Username(rabbit["Username"] ?? "guest");
                        host.Password(rabbit["Password"] ?? "guest");
                    });
                    cfg.UseMessageRetry(retry => retry.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                bus.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
        });

        return serviceCollection;
    }
}
