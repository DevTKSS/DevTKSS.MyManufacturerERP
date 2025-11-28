namespace DevTKSS.MyManufacturerERP.Server.Validation.Identity;

public class CustomUserValidator<TUser> : IUserValidator<TUser> where TUser : class
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        // Hier können Sie Ihre benutzerdefinierte Benutzer-Validierung implementieren.
        // Beispiel: Benutzername darf nicht leer sein
        var userName = manager.GetUserNameAsync(user).Result;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "UserNameEmpty",
                Description = "Der Benutzername darf nicht leer sein."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}