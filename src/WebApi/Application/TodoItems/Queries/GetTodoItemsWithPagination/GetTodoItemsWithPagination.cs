namespace DevTKSS.MyManufacturerERP.Application.TodoItems.Queries.GetTodoItemsWithPagination;

public record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTodoItemsWithPaginationQueryHandler : IRequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TypeAdapterConfig _mapsterConfig;

    public GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context, TypeAdapterConfig mapsterConfig)
    {
        _context = context;
        _mapsterConfig = mapsterConfig;
    }

    public async ValueTask<PaginatedList<TodoItemBriefDto>> Handle(GetTodoItemsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var itemsQuery = _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title);

        var projected = await itemsQuery.ProjectToTypeListAsync<TodoItem, TodoItemBriefDto>(_mapsterConfig, cancellationToken);

        return await PaginatedList<TodoItemBriefDto>.CreateAsync(projected.AsQueryable(), request.PageNumber, request.PageSize, cancellationToken);
    }
}
