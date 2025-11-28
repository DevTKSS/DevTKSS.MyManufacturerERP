using DevTKSS.MyManufacturerERP.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DevTKSS.MyManufacturerERP.Application.TodoItems.EventHandlers;

public class TodoItemCreatedEventHandler : INotificationHandler<TodoItemCreatedEvent>
{
    private readonly ILogger<TodoItemCreatedEventHandler> _logger;

    public TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DevTKSS Domain Event: {DomainEvent}", notification.GetType().Name);

        return ValueTask.CompletedTask;
    }
}
