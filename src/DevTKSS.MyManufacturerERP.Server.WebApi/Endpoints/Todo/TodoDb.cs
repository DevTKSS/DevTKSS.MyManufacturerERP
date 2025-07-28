namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Todo;

public class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options)
    {
    }
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

}
