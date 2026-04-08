namespace DevTKSS.Extensions.OAuth.UI.Navigation;

public interface IOAuthNavigationService // HACK: Needed until Issue https://github.com/unoplatform/uno.extensions/issues/3015 provides a Window INavigator implementation beside just Dialog
{
    /// <summary>
    /// Gets whether the Authentication Navigation is currently running.
    /// </summary>
    bool IsNavigatingAuthentication { get; }
    /// <summary>
    /// Navigates an Interactive OAuth authentication flow, using the <see cref="INavigator.NavigateDataForResultAsync{TRequest,TResult}"/> of the provided <see cref="INavigator"/> to trigger the flow and retrieve the result, which is then exchanged for the final Token Response and returned.:
    /// </summary>
    /// <param name="navigator">A regular <see cref="INavigator"/> Instance used to navigate the requested flow.</param>
    /// <param name="sender">The sender of the Navigation flow.</param>
    /// <param name="qualifier">A <see cref="string"/> representing the desired navigation UI Element to nest in or call.<br/>
    /// Choose from: <see cref="OAuthNavigationQualifiers.SystemBrowser"/>, <see cref="Qualifiers.Dialog"/> or <see cref="OAuthNavigationQualifiers.Window"/>
    /// </param>
    /// <param name="extraParameters">Additional query parameters to include with the authorization request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the Authentication Flow.</param>
    /// <returns></returns> // TODO: Decide what should be the return.  Decide along SoC and optimal layering?
    ValueTask<TokenResponse?> NavigateAuthenticationAsync( // TODO: Provide proper intereactive flows (Dialog/Window/System Browser) + proper handling of available options per the platform (mobile is limited in Multi Window support, prefer Dialogs or simply System Browser here, Desktop has problems with Systembrowser, but WebView2 displays flawless).
        INavigator navigator,
        string qualifier,
        object? sender = null, // NOTE: 'object Sender' is required from Uno.Extensions.Navigation NavigationResponse to trigger navigation, CAN NOT be actually null! Trigger this method / flow from the ViewModels / MVUX Model, unknown if it can be also triggered from Services itself. `Sender` Reference: https://github.com/unoplatform/uno.extensions/blob/85da926ae0b08aec198606f1b8b18d8d0a475d91/src/Uno.Extensions.Navigation/NavigationRequest.cs#L4
       // object? data = null, // TODO: potentially allow providing the HttpClient / AccessTokenProvider or similar for non-interactive flows? But this would again require the caller to get and insert it, lowers modularity and SoC (NavigatorExtensions Reference: https://github.com/unoplatform/uno.extensions/blob/85da926ae0b08aec198606f1b8b18d8d0a475d91/src/Uno.Extensions.Navigation/NavigatorExtensions.cs#L181-L205)
        IDictionary<string, string>? extraParameters = null,
        CancellationToken cancellationToken = default);
    // TODO: How to implement Property `Route` / Method `Task<bool> CanNavigate()` of the Interface INavigator (Reference: https://github.com/unoplatform/uno.extensions/blob/85da926ae0b08aec198606f1b8b18d8d0a475d91/src/Uno.Extensions.Navigation/INavigator.cs)
}
