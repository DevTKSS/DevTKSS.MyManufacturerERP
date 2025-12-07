namespace DevTKSS.MyManufacturerERP.Services.Endpoints;

internal class DebugHttpHandler : DelegatingHandler
{
    public DebugHttpHandler(HttpMessageHandler? innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {
    }

    protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
#if DEBUG
        if (!response.IsSuccessStatusCode)
        {
            Log.Debug("Unsuccessful API Call");
            if (request.RequestUri is not null)
            {
                Log.Debug($"{request.RequestUri} ({request.Method})");
            }
            
            foreach ((var key, var values) in request.Headers.ToDictionary(x => x.Key, x => string.Join(", ", x.Value)))
            {
               Log.Debug($"{key}: {values}");
            }

            var content = request.Content is not null ? await request.Content.ReadAsStringAsync() : null;
            if (!string.IsNullOrEmpty(content))
            {
                Log.Debug(content);
            }

            // Uncomment to automatically break when an API call fails while debugging
            // System.Diagnostics.Debugger.Break();
        }
#endif
        return response;
    }
}
