namespace DevTKSS.Extensions.OAuth.Validation;

public class FluentValidateOptions<TOptions>
    : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;
    public FluentValidateOptions(IServiceProvider serviceProvider, string? name)
    {
        _serviceProvider = serviceProvider;
        _name = name;
    }
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if(_name != null && name != _name)
        {
            // Ignored if not validating this instance.
            return ValidateOptionsResult.Skip;
        }

        ArgumentNullException.ThrowIfNull(options);

        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();
        var result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }
        var type = options.GetType().Name;
        var errors = result.Errors
            .Select(failure => $"Options validation for type '{type}' failed for '{failure.PropertyName}' with error: {failure.ErrorMessage}")
            .ToList();
        return ValidateOptionsResult.Fail(errors);
    }
}