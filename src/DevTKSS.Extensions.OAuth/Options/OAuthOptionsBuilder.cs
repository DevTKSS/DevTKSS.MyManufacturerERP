namespace DevTKSS.Extensions.OAuth.Options;

public class OAuthOptionsBuilder
{

    private string? _clientID;
    private string[] _scopes = [];
    private string _providerName = OAuthOptions.DefaultName;
    private string? _baseUrl;
    private string? _clientSecret;
    private readonly OAuthOptions? preConfiguredOptions;
    private OAuthEndpointOptions? _endpointOptions;
    private OAuthOptionsBuilder(OAuthOptions? options = null)
    {
        preConfiguredOptions = options;
    }
    public static OAuthOptionsBuilder Create(OAuthOptions? options = null)
    => new (options);

    public OAuthOptionsBuilder ConfigureEndpoints(Action<OAuthEndpointOptionsBuilder> configure)
    {
        var endpointBuilder = OAuthEndpointOptionsBuilder.Create(preConfiguredOptions);
        configure(endpointBuilder);
        _endpointOptions = endpointBuilder.Build();
        return this;
    }
    public OAuthOptionsBuilder WithProviderName(string providerName)
    {
        _providerName = providerName;
        return this;
    }
   
    public OAuthOptionsBuilder WithBaseAddress(string baseAddress)
    {
        if(string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentException("Base address cannot be null or empty.", nameof(baseAddress));
        if (!baseAddress.EndsWith('/'))
            baseAddress += '/';
        if(!Uri.TryCreate(baseAddress,UriKind.Relative,out var relativeUri)) 
            throw new InvalidOperationException("AuthorizationEndpoint must be a valid relative URI.");
        _baseUrl = baseAddress;

        return this;
    }
    public OAuthOptionsBuilder WithCallbackUri(string callbackUri)
    {
        
        return this;
    }
    public OAuthOptionsBuilder WithClientID(string clientId)
    {
        _clientID = clientId;
        return this;
    }
    /// <summary>
    /// Sets the client secret for the OAuth client.
    /// </summary>
    /// <remarks>This method is intended for use with confidential clients that can securely store the client
    /// secret.  Public clients, such as mobile or desktop applications, typically do not use a client secret as they 
    /// cannot guarantee its confidentiality.</remarks>
    /// <param name="clientSecret">The client secret to be used for authentication. This is typically required for confidential clients,  such as
    /// web applications or services, that need to authenticate securely with the OAuth provider.</param>
    /// <returns>The current <see cref="OAuthOptionsBuilder"/> instance, allowing for method chaining.</returns>
    public OAuthOptionsBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    public OAuthOptionsBuilder WithScopes(string[] scopes)
    {
        _scopes = scopes;
        return this;
    }
    public OAuthOptions Build()
    {
        return new OAuthOptions
        {
            ProviderName = _providerName,
            ClientID = _clientID,
            Scopes = _scopes,
            Url = _baseUrl,
            ClientSecret = _clientSecret,
            EndpointOptions = _endpointOptions ?? preConfiguredOptions?.EndpointOptions ?? new(),
        };

    }
}
