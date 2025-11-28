using DevTKSS.MyManufacturerERP.Application.Common.Interfaces;

namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Todo;

public class TodoDb : DbContext, IApplicationDbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options)
    {
    }
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<TodoList> TodoLists => Set<TodoList>();
}
