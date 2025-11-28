namespace DevTKSS.MyManufacturerERP.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull, IMessage
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            if (_logger.IsEnabled(LogLevel.Error))
            { 
                _logger.LogError(ex, "DevTKSS Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
            }

            throw;
        }
    }
}
