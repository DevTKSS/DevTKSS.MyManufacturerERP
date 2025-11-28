using DevTKSS.MyManufacturerERP.Application.Common.Security;
using DevTKSS.MyManufacturerERP.Application.Common.Interfaces;

namespace DevTKSS.MyManufacturerERP.Application.TodoLists.Commands.PurgeTodoLists;

[Authorize(Roles = Rule.Administrator)]
[Authorize(Policy = Policies.CanPurge)]
public record PurgeTodoListsCommand : ICommand;

public class PurgeTodoListsCommandHandler : ICommandHandler<PurgeTodoListsCommand>
{
    private readonly IApplicationDbContext _context;

    public PurgeTodoListsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(PurgeTodoListsCommand request, CancellationToken cancellationToken)
    {
        _context.TodoLists.RemoveRange(_context.TodoLists);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
