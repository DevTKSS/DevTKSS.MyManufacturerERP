# DevTKSS.MyManufacturerERP.Server

## Issues

### Workarounds

- [Uno Platform GitHub Issue #20546](https://github.com/unoplatform/uno/issues/20546) - String TypeInfo serialization

### Uno Platform Documentation Problems

- [Uno Platform Post-Login Token Processing](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#4-process-post-login-tokens) - Process post-login tokens
  while the docs are introducing us to do this for web auth, [this is not working as documented](./DevTKSS.MyManufacturerERP/App.xaml.cs#L116)

  ```bash
  CS1593
  Delegate "AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?>" does not accept 3 arguments.
  DevTKSS.MyManufacturerERP (net9.0-browserwasm)
  C:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\App.xaml.cs 116
  ```
  ![Screenshot of IDE Type info for parameters](./Images/IDE-Login-Typeinfo-screenshot.png)]

- [Uno Platform Cookie Authentication](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-Cookies.html) - Configure cookie options

  This is missing kind of all docs about working with cookies. The provided links are not very helpfull for learning only if you might not know the words, but still you will not know how to use that in development.

### Authentication & Server Issues

- Server startup problems when clicking "Login" button in client application
- Port configuration conflicts between WebAssembly and Server targets
- Missing Identity API endpoints in Endpoint Explorer
- Delegate signature mismatch in Uno Platform authentication [CS1593 error](#uno-platform-documentation-problems)

## Usefull References

- [Browser Link](https://learn.microsoft.com/en-us/aspnet/core/client-side/using-browserlink?view=aspnetcore-9.0)
- [Debugging ASP.NET Core Launch Settings incl. Ports](https://learn.microsoft.com/en-us/visualstudio/debugger/how-to-enable-debugging-for-aspnet-applications?view=vs-2022#debug-aspnet-core-apps)
- [Fundamentals of ASP.NET environment specific settings](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-9.0)

## Documentation & Learning References

- [ASP.NET Core Cookie Authentication Guide](https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0) - Cookie authentication implementation
- [ASP.NET Core Policy Schemes Documentation](https://learn.microsoft.com/de-de/aspnet/core/security/authentication/policyschemes?view=aspnetcore-9.0) - Authentication forwarding configuration
- [ASP.NET Core Minimal Web API Tutorial](https://learn.microsoft.com/de-de/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0&tabs=visual-studio) - TodoList example implementation
- [Uno Platform Web Authentication Guide](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#3-configure-the-provider) - Web authentication provider configuration

## API Documentation & Services

- [Etsy OAuth Token Requests](https://developers.etsy.com/documentation/essentials/authentication#requesting-an-oauth-token) - CSRF protection requirements
- [SevDesk API Authentication Reference](https://api.sevdesk.de/#section/Authentication-and-Authorization) - Token-based authentication

## CORS & WebAssembly

- [MDN CORS Documentation](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS) - Cross-origin resource sharing
- [MDN CORS Preflight Requests](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS#preflighted_requests) - Preflight request headers
- [WebAssembly Multi-threading Features](https://github.com/dotnet/runtime/blob/main/src/mono/wasm/features.md#multi-threading) - WASM threading support
- [Substack CORS Configuration Reference](https://substack.com/home/post/p-164745710) - CORS policy setup
