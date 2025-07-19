namespace DevTKSS.MyManufacturerERP.Server.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AuthDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log the error or handle it as needed
            throw new Exception("An error occurred while applying migrations.", ex);
        }
    }
}
