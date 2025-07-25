namespace DevTKSS.MyManufacturerERP.Server.WebApi.Database;
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<User>(options)
{
}