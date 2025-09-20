using System.ComponentModel;

namespace Uno.Extensions.Authentication;

/// <summary>
/// Provides OAuth-related extension methods for <see cref="IOAuthAuthenticationBuilder"/>.
/// </summary>
public static class OAuthAuthenticationBuilderExtensions
{

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified login start URI.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="uri">
	/// The login start URI to use.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder LoginStartUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		string uri)
		where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder
		=>
			builder.Property((OAuthSettings s)
				=> s with { LoginStartUri = uri });

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login start URI. 
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginStartUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate that will prepare the login start URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginStartUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<string> prepare)
		where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>
			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginStartUri = (services, cache, loginStartUri, cancellationToken) =>
									prepare(cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login start URI. This overload allows
	/// for a delegate that will use a service provider for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginStartUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate, which takes a service provider and returns a string, that will prepare the login start URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginStartUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginStartUri = (services, cache, loginStartUri, cancellationToken) =>
									prepare(services, cancellationToken)
				});


	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login start URI. This overload allows
	/// for a delegate that will use a service provider, a token cache, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginStartUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service provider, a token cache, a dictionary of tokens, and returns a string—that will prepare the login start URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginStartUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, ITokenCache, string?, string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginStartUri = prepare
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login start URI. This overload allows
	/// for a delegate that will use a service of the specified type for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PrepareLoginStartUri"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the login start URI.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service of type <typeparamref name="TService"/> and returns a string—that will prepare the login start URI.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PrepareLoginStartUri<TService>(
	this IOAuthAuthenticationBuilder<TService> builder,
	AsyncFunc<TService, string> prepare)
		where TService : notnull =>
			builder.Property((OAuthSettings<TService> s)
			=> s with
			{
				PrepareLoginStartUri = (service, services, cache, loginStartUri, cancellationToken) =>
								prepare(service, cancellationToken)
			});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login start URI. This overload allows
	/// for a delegate that will use a service of the specified type, a service provider, a token cache reference, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PrepareLoginStartUri"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the login start URI.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service of type <typeparamref name="TService"/>, a service provider, a token cache reference, and returns a string—that will prepare the login start URI.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PrepareLoginStartUri<TService>(
	this IOAuthAuthenticationBuilder<TService> builder,
	AsyncFunc<TService, IServiceProvider, ITokenCache, string?, string> prepare)
		where TService : notnull =>
			builder.Property((OAuthSettings<TService> s)
			=> s with
			{
				PrepareLoginStartUri = prepare
			});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified login callback URI.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="uri">
	/// The login callback URI to use.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder LoginCallbackUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		string uri)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with { LoginCallbackUri = uri });

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginCallbackUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginCallbackUri = (services, cache, loginCallbackUri, cancellationToken) =>
									prepare(cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI. This overload allows
	/// for a delegate that will use a service provider for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate, which takes a service provider and returns a string, that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginCallbackUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginCallbackUri = (services, cache,loginCallbackUri, cancellationToken) =>
									prepare(services, cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI. This overload allows
	/// for a delegate that will use a service provider and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service provider, the LoginStartUri and returns a string—that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginCallbackUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, string?, string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginCallbackUri = (services, cache, loginCallbackUri, cancellationToken) =>
									prepare(services, loginCallbackUri, cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI. This overload allows
	/// for a delegate that will use a service provider, a token cache, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service provider, a token cache, a dictionary of tokens, and returns a string—that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PrepareLoginCallbackUri<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, ITokenCache, string?, string> prepare)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PrepareLoginCallbackUri = prepare
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI. This overload allows
	/// for a delegate that will use a service of the specified type for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the login callback URI.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service of type <typeparamref name="TService"/> and returns a string—that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PrepareLoginCallbackUri<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		AsyncFunc<TService, string> prepare)
		where TService : notnull =>
			builder.Property((OAuthSettings<TService> s)
				=> s with
				{
					PrepareLoginCallbackUri = (service, services, cache, loginCallbackUri, cancellationToken) =>
									prepare(service, cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with a delegate that will prepare the login callback URI. This overload allows
	/// for a delegate that will use a service of the specified type, a service provider, a token cache reference, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PrepareLoginCallbackUri"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the login callback URI.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="prepare">
	/// A delegate—which takes a service of type <typeparamref name="TService"/>, a service provider, a token cache reference, a dictionary of tokens, and returns a string—that will prepare the login callback URI.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PrepareLoginCallbackUri<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		AsyncFunc<TService, IServiceProvider, ITokenCache, string?, string> prepare)
		where TService : notnull =>
			builder.Property((OAuthSettings<TService> s)
				=> s with
				{
					PrepareLoginCallbackUri = prepare
				});
    /// <summary>
    /// Configures the OAuth authentication feature to be built with the specified post login callback. This overload allows
    /// for a delegate that will use a service of the specified type, a service provider, a token cache reference, a dictionary of credentials, and a dictionary of tokens for authentication.
    /// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PostLoginCallback"/>.
    /// </summary>
    /// <param name="builder">
    /// The instance of <see cref="IWebAuthenticationBuilder"/> to configure.
    /// </param>
    /// <param name="postLogin">
    /// A delegate—which takes a service provider, a token cache reference, a dictionary of credentials, a dictionary of tokens, and returns a dictionary of tokens (or null)—that will prepare the return value of the post login callback.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IWebAuthenticationBuilder"/> that was passed in.
    /// </returns>
    public static IOAuthAuthenticationBuilder PostLogin<TOAuthAuthenticationBuilder>(
        this TOAuthAuthenticationBuilder builder,
        AsyncFunc<IServiceProvider, ITokenCache, string, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
            where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>
                builder.Property((OAuthSettings s)
                    => s with { PostLoginCallback = postLogin });

    /// <summary>
    /// Configures the OAuth authentication feature to be built with the specified post login callback.
    /// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PostLoginCallback"/>.
    /// </summary>
    /// <typeparam name="TOAuthAuthenticationBuilder">
    /// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
    /// </typeparam>
    /// <param name="builder">
    /// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
    /// </param>
    /// <param name="postLogin">
    /// A delegate that uses a dictionary of tokens and returns a dictionary of tokens (or null) that can be used for post login operations.
    /// </param>
    /// <returns>
    /// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
    /// </returns>
    public static TOAuthAuthenticationBuilder PostLogin<TOAuthAuthenticationBuilder>(
	this TOAuthAuthenticationBuilder builder,
	AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?> postLogin)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

		builder.Property((OAuthSettings s)
			=> s with
			{
				PostLoginCallback = (services, cache, redirectUri, tokens, cancellationToken) =>
								postLogin(tokens, cancellationToken)
			});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified post login callback. This overload allows
	/// for a delegate that will use a service provider, a token cache reference, a dictionary of credentials, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.PostLoginCallback"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="postLogin">
	/// A delegate that uses a service provider, a token cache reference, a dictionary of credentials, a dictionary of tokens, and returns a dictionary of tokens (or null) that can be used for post login operations.
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder PostLogin<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with
				{
					PostLoginCallback = (services, cache, redirectUri, tokens, cancellationToken) =>
									postLogin(services, cache, tokens, cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified post login callback. This overload allows
	/// for a delegate that will use a service of the specified type, a dictionary of tokens, and return a dictionary of tokens (or null) for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PostLoginCallback"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the return value of the post login callback.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="postLogin">
	/// A delegate—which takes a service of type <typeparamref name="TService"/>, a dictionary of tokens, and returns a dictionary of tokens (or null)—that will prepare the return value of the post login callback.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PostLogin<TService>(
this IOAuthAuthenticationBuilder<TService> builder,
AsyncFunc<TService, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
		where TService : notnull =>
			builder.Property((OAuthSettings<TService> s)
		=> s with
		{
			PostLoginCallback = (service, services, cache, redirectUri, tokens, cancellationToken) =>
							postLogin(service, tokens, cancellationToken)
		});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified post login callback. This overload allows
	/// for a delegate that will use a service of the specified type, a service provider, a token cache reference, a dictionary of credentials, and a dictionary of tokens for authentication.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.PostLoginCallback"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to prepare the return value of the post login callback.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="postLogin">
	/// A delegate—which takes a service of type <typeparamref name="TService"/>, a service provider, a token cache reference, a dictionary of credentials, a dictionary of tokens, and returns a dictionary of tokens (or null)—that will prepare the return value of the post login callback.
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> PostLogin<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		AsyncFunc<TService, IServiceProvider, ITokenCache, string, IDictionary<string, string>, IDictionary<string, string>?> postLogin)
			where TService : notnull =>
				builder.Property((OAuthSettings<TService> s)
					=> s with { PostLoginCallback = postLogin });

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified refresh callback. This type of callback is used to refresh
	/// the tokens that are used for authentication when they expire.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.RefreshCallback"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="refreshCallback">
	/// A delegate that uses a dictionary of tokens and returns a dictionary of the updated tokens (or null).
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder Refresh<TOAuthAuthenticationBuilder>(
	this TOAuthAuthenticationBuilder builder,
	AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?> refreshCallback)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

		builder.Property((OAuthSettings s)
			=> s with
			{
				RefreshCallback = (services, cache, tokens, cancellationToken) =>
			refreshCallback(tokens, cancellationToken)
			});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified refresh callback. This type of callback is used to refresh
	/// the tokens that are used for authentication when they expire. This overload allows for a delegate that will use a service provider,
	/// a token cache reference, and a dictionary of existing tokens.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings.RefreshCallback"/>.
	/// </summary>
	/// <typeparam name="TOAuthAuthenticationBuilder">
	/// The type of <see cref="IOAuthAuthenticationBuilder"/> implementation that will be configured.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation to configure.
	/// </param>
	/// <param name="refreshCallback">
	/// A delegate that uses a service provider, a token cache reference, a dictionary of existing tokens, and returns a dictionary of the updated tokens (or null).
	/// </param>
	/// <returns>
	/// An instance of the <see cref="IOAuthAuthenticationBuilder"/> implementation that was passed in.
	/// </returns>
	public static TOAuthAuthenticationBuilder Refresh<TOAuthAuthenticationBuilder>(
		this TOAuthAuthenticationBuilder builder,
		AsyncFunc<IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?> refreshCallback)
				where TOAuthAuthenticationBuilder : IOAuthAuthenticationBuilder =>

			builder.Property((OAuthSettings s)
				=> s with { RefreshCallback = refreshCallback });

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified refresh callback. This type of callback is used to refresh
	/// the tokens that are used for authentication when they expire. This overload allows for a delegate that will use a service of the
	/// specified type and a dictionary of existing tokens.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.RefreshCallback"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to refresh the tokens.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="refreshCallback">
	/// A delegate—which takes a service of type <typeparamref name="TService"/> as well as a dictionary of existing tokens and returns a dictionary of the updated tokens (or null).
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> Refresh<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		AsyncFunc<TService, IDictionary<string, string>, IDictionary<string, string>?> refreshCallback)
			where TService : notnull =>
			builder.Property((WebAuthenticationSettings<TService> s)
				=> s with
				{
					RefreshCallback = (service, services, cache, tokens, cancellationToken) =>
						refreshCallback(service, tokens, cancellationToken)
				});

	/// <summary>
	/// Configures the OAuth authentication feature to be built with the specified refresh callback. This type of callback is used to refresh
	/// the tokens that are used for authentication when they expire. This overload allows for a delegate that will use a service of the
	/// specified type, a service provider, a token cache reference, and a dictionary of existing tokens.
	/// The underlying property that will be set to such delegate is located on <see cref="OAuthSettings{TService}.RefreshCallback"/>.
	/// </summary>
	/// <typeparam name="TService">
	/// The type of service that will be used by the delegate to refresh the tokens.
	/// </typeparam>
	/// <param name="builder">
	/// The instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> to configure.
	/// </param>
	/// <param name="refreshCallback">
	/// A delegate—which takes a service of type <typeparamref name="TService"/>, a service provider, a token cache reference, a dictionary of existing tokens, and returns a dictionary of the updated tokens (or null).
	/// </param>
	/// <returns>
	/// An instance of <see cref="IOAuthAuthenticationBuilder{TService}"/> that was passed in.
	/// </returns>
	public static IOAuthAuthenticationBuilder<TService> Refresh<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		AsyncFunc<TService, IServiceProvider, ITokenCache, IDictionary<string, string>, IDictionary<string, string>?> refreshCallback)
			where TService : notnull =>
			builder.Property((WebAuthenticationSettings<TService> s)
				=> s with { RefreshCallback = refreshCallback });

	// Add an advanced builder allowing to tweak the options directly
	/// <summary>
	/// Configures the OAuth authentication feature by updating directly the <see cref="TokenCacheOptions"/> parameter.
	/// </summary>
	/// <remarks>
	/// The <see cref="IOAuthAuthenticationBuilder"/> with updated <see cref="TokenCacheOptions"/> will be returned.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static IOAuthAuthenticationBuilder<TService> ConfigureTokenCacheKeys<TService>(
		this IOAuthAuthenticationBuilder<TService> builder,
		Action<TokenCacheOptions> configure)
		where TService : notnull
	{
		if (builder is IBuilder<OAuthSettings> authBuilder)
		{
			var tcOptions = authBuilder.Settings.TokenCacheOptions;
			configure.Invoke(tcOptions);
			authBuilder.Settings = authBuilder.Settings with
			{
				TokenCacheOptions = tcOptions
			};
		}

		return builder;
	}
	// Add an advanced builder allowing to tweak the options directly
	/// <summary>
	/// Configures the OAuth authentication feature by updating directly the <see cref="TokenCacheOptions"/> parameter.
	/// </summary>
	/// <remarks>
	/// The <see cref="IOAuthAuthenticationBuilder"/> with updated <see cref="TokenCacheOptions"/> will be returned.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static IOAuthAuthenticationBuilder ConfigureTokenCacheKeys(
		this IOAuthAuthenticationBuilder builder,
		Action<TokenCacheOptions> configure)
	{
		if (builder is IBuilder<OAuthSettings> authBuilder)
		{
			var tcOptions = authBuilder.Settings.TokenCacheOptions;
			configure.Invoke(tcOptions);
			authBuilder.Settings = authBuilder.Settings with
			{
				TokenCacheOptions = tcOptions
			};
		}

		return builder;
	}
	private static TBuilder Property<TBuilder, TSettings>(
		this TBuilder builder,
		Func<TSettings, TSettings> setProperty)
		where TBuilder : IBuilder
		where TSettings : class, new()
	{
		if (builder is IBuilder<TSettings> authBuilder)
		{
			authBuilder.Settings = setProperty(authBuilder.Settings);
		}

		return builder;
	}
}
