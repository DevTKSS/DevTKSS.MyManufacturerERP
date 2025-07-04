using Microsoft.AspNetCore.Identity;
namespace DevTKSS.MyManufacturerERP.Server.Database;

public class User : IdentityUser
{
    public string? Initials { get; set; }
}
