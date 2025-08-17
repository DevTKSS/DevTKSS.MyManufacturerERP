namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

public class AuthDb : DbContext
{
    public AuthDb(DbContextOptions<AuthDb> options)
        : base(options)
    {
    }
    // Define DbSets for your entities here, e.g.:
   public DbSet<User> Users => Set<User>();
}

