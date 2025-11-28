namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Todo;
public static class TodoItemEndpoints
{
    public static RouteGroupBuilder MapTodoEnpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todoitems")
            .WithTags("TodoItems");

        group.MapGet("/", GetAllTodoItems)
            .WithName("GetTodoItems")
            .WithSummary("Get all todo items")
            .WithDescription("Retrieve a list of all todo items");

        group.MapGet("/complete", GetCompletedTodoItems)
            .WithName("GetCompletedTodoItems")
            .WithSummary("Get completed todo items")
            .WithDescription("Retrieve a list of all completed todo items");

        group.MapGet("/{id:int}", GetTodoItemById)
            .WithName("GetTodoItemById")
            .WithSummary("Get todo item by ID")
            .WithDescription("Retrieve a specific todo item by its ID");

        group.MapPost("/", CreateTodoItem)
            .WithName("CreateTodoItem")
            .WithSummary("Create a new todo item")
            .WithDescription("Create a new todo item");

        group.MapPut("/{id:int}", UpdateTodoItem)
            .WithName("UpdateTodoItem")
            .WithSummary("Update todo item")
            .WithDescription("Update an existing todo item");

        group.MapDelete("/{id:int}", DeleteTodoItem)
            .WithName("DeleteTodoItem")
            .WithSummary("Delete todo item")
            .WithDescription("Delete a todo item by its ID");

        return group;
    }

    private static async Task<Ok<List<TodoItem>>> GetAllTodoItems(TodoDb db)
    {
        var items = await db.TodoItems.ToListAsync();
        return TypedResults.Ok(items);
    }

    private static async Task<Ok<List<TodoItem>>> GetCompletedTodoItems(TodoDb db)
    {
        var items = await db.TodoItems.Where(t => t.Done).ToListAsync();
        return TypedResults.Ok(items);
    }

    private static async Task<Results<Ok<TodoItem>, NotFound>> GetTodoItemById(int id, TodoDb db)
    {
        var item = await db.TodoItems.FindAsync(id);
        return item is not null
            ? TypedResults.Ok(item)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<TodoItem>, ValidationProblem>> CreateTodoItem(
        TodoItem todo, TodoDb db)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required"]
            });
        }
        var existingTodo = await db.TodoItems
            .AnyAsync(t => t.Title == todo.Title);
        if (existingTodo)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Name"] = ["A todo item with this name already exists"]
            });
        }
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/{todo.Id}", todo);
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> UpdateTodoItem(
        int id, TodoItem inputTodo, TodoDb db)
    {
        if (string.IsNullOrWhiteSpace(inputTodo.Title))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required"]
            });
        }

        var existingTodo = await db.TodoItems.FindAsync(id);
        if (existingTodo is null)
        {
            return TypedResults.NotFound();
        }

        existingTodo.Title = inputTodo.Title;
        existingTodo.Done = inputTodo.Done;

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTodoItem(int id, TodoDb db)
    {
        var todo = await db.TodoItems.FindAsync(id);
        if (todo is null)
        {
            return TypedResults.NotFound();
        }

        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
