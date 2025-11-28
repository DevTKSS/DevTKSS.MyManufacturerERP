namespace DevTKSS.MyManufacturerERP.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest, TResponse> : MessagePreProcessor<TRequest, TResponse>
    where TRequest : notnull, IMessage
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    protected override ValueTask Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = _identityService.GetUserNameAsync(userId).GetAwaiter().GetResult();
        }

        _logger.LogInformation("DevTKSS Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);

        return default;
    }
}
