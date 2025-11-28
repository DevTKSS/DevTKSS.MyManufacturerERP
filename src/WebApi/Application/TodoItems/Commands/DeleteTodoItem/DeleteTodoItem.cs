using DevTKSS.MyManufacturerERP.Application.Common.Interfaces;
using DevTKSS.MyManufacturerERP.Domain.Events;

namespace DevTKSS.MyManufacturerERP.Application.TodoItems.Commands.DeleteTodoItem;

public record DeleteTodoItemCommand(int Id) : ICommand;

public class DeleteTodoItemCommandHandler : ICommandHandler<DeleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.TodoItems.Remove(entity);

        entity.AddEntityEvent(new TodoItemDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }

}
