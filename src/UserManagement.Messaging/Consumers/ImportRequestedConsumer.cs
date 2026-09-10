using MassTransit;
using UserManagement.Services.Messaging.Messages;
using UserManagement.Services.Services.ImportServices;

namespace UserManagement.Messaging.Consumers;

public sealed class ImportRequestedConsumer(ImportProcessor processor) : IConsumer<ImportRequested>
{
    public Task Consume(ConsumeContext<ImportRequested> context)
        => processor.ProcessAsync(context.Message.JobId, context.CancellationToken);
}
