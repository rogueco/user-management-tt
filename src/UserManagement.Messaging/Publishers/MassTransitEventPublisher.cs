using MassTransit;
using UserManagement.Services.Messaging;

namespace UserManagement.Messaging.Publishers;

internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => publishEndpoint.Publish(message, cancellationToken);
}
