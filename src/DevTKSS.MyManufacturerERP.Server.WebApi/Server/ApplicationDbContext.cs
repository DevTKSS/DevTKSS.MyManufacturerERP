using OpenIddict.EntityFrameworkCore.Models;

namespace DevTKSS.MyManufacturerERP.Web.Server;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; } = null!;
}
