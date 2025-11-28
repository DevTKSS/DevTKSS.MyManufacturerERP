namespace DevTKSS.MyManufacturerERP.Application.TodoLists.Queries.GetTodos;

public class TodosVm
{
    public IReadOnlyCollection<LookupDto> PriorityLevels { get; init; } = [];

    public IReadOnlyCollection<TodoListDto> Lists { get; init; } = [];
}
