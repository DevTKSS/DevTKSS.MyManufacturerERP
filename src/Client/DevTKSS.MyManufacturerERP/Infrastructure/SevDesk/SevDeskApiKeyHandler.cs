namespace DevTKSS.MyManufacturerERP.Infrastructure.SevDesk;

/// <summary>
/// Attaches the SevDesk API token as the Authorization header on every outgoing request.
/// See https://api.sevdesk.de/#section/Authentication-and-Authorization
/// </summary>
public sealed class SevDeskApiKeyHandler : DelegatingHandler
{
    private readonly SevDeskClientOptions _options;
    private readonly ILogger<SevDeskApiKeyHandler> _logger;

    public SevDeskApiKeyHandler(
        IOptions<SevDeskClientOptions> options,
        ILogger<SevDeskApiKeyHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("SevDesk API key is not configured; requests will likely fail.");
        }
        else
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_options.ApiKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
