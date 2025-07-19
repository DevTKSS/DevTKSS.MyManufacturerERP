namespace DevTKSS.MyManufacturerERP.Server.WebApi.Database;
public class AuthDbContext : IdentityDbContext<User>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

}