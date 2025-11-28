namespace DevTKSS.MyManufacturerERP.Server.Api.Identity.Validation;

public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        // Hier können Sie Ihre benutzerdefinierte Passwortvalidierung implementieren.
        // Beispiel: Mindestlänge prüfen
        if (string.IsNullOrEmpty(password) || password.Length < 12)
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Das Passwort muss mindestens 12 Zeichen lang sein."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
