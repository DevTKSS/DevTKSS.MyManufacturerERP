
using DevTKSS.MyManufacturerERP.Application.Common.Interfaces;
using DevTKSS.MyManufacturerERP.Application.Common.Mappings;
using Mapster;

namespace DevTKSS.MyManufacturerERP.Application.TodoLists.Queries.GetTodos;

[Authorize]
public record GetTodosQuery : IRequest<TodosVm>;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;
    private readonly TypeAdapterConfig _mapsterConfig;

    public GetTodosQueryHandler(IApplicationDbContext context, TypeAdapterConfig mapsterConfig)
    {
        _context = context;
        _mapsterConfig = mapsterConfig;
    }

    public async ValueTask<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var lists = await _context.TodoLists
            .AsNoTracking()
            .OrderBy(t => t.Title)
            .ProjectToTypeListAsync<TodoList, TodoListDto>(_mapsterConfig, cancellationToken);

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues<PriorityLevel>()
                .Cast<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToList(),

            Lists = lists
        };
    }
}
