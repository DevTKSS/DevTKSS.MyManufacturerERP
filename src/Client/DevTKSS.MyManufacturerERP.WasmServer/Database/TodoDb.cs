namespace DevTKSS.MyManufacturerERP.Server.Database;

public class TodoDb(DbContextOptions<TodoDb> options)
    : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

}
