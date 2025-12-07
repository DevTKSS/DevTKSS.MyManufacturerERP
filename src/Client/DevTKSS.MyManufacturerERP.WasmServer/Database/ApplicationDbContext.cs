using DevTKSS.MyManufacturerERP.Server.Models;

namespace DevTKSS.MyManufacturerERP.Server.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
}

