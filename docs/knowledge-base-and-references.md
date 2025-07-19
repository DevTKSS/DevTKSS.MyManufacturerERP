# DevTKSS.MyManufacturerERP.Server

This document contains additional references and known issues related to the DevTKSS.MyManufacturerERP project, particularly focusing on the Uno Platform and its integration with ASP.NET Core Identity.

## Table of Contents

- [DevTKSS.MyManufacturerERP.Server](#devtkssmymanufacturererpserver)
  - [Table of Contents](#table-of-contents)
  - [Useful References](#useful-references)
  - [Documentation \& Learning References](#documentation--learning-references)
  - [API Documentation \& Services](#api-documentation--services)
  - [CORS \& WebAssembly](#cors--webassembly)

---

## Useful References

- [Browser Link](https://learn.microsoft.com/en-us/aspnet/core/client-side/using-browserlink?view=aspnetcore-9.0)
- [Debugging ASP.NET Core Launch Settings incl. Ports](https://learn.microsoft.com/en-us/visualstudio/debugger/how-to-enable-debugging-for-aspnet-applications?view=vs-2022#debug-aspnet-core-apps)
- [Fundamentals of ASP.NET environment specific settings](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-9.0)

## Documentation & Learning References

- [ASP.NET Core Cookie Authentication Guide](https://learn.microsoft.com/de-de/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0) - Cookie authentication implementation
- [ASP.NET Core Policy Schemes Documentation](https://learn.microsoft.com/de-de/aspnet/core/security/authentication/policyschemes?view=aspnetcore-9.0) - Authentication forwarding configuration
- [ASP.NET Core Minimal Web API Tutorial](https://learn.microsoft.com/de-de/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0&tabs=visual-studio) - TodoList example implementation
- [Uno Platform Web Authentication Guide](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#3-configure-the-provider) - Web authentication provider configuration
- [A comprehensive overview of ASP.NET Core Authentication](https://www.reddit.com/r/dotnet/comments/we9qx8/a_comprehensive_overview_of_authentication_in/) - Reddit article about ASP.NET Core authentication

## API Documentation & Services

- [Etsy OAuth Token Requests](https://developers.etsy.com/documentation/essentials/authentication#requesting-an-oauth-token) - CSRF protection requirements
- [SevDesk API Authentication Reference](https://api.sevdesk.de/#section/Authentication-and-Authorization) - Token-based authentication

## CORS & WebAssembly

- [MDN CORS Documentation](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS) - Cross-origin resource sharing
- [MDN CORS Preflight Requests](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS#preflighted_requests) - Preflight request headers
- [WebAssembly Multi-threading Features](https://github.com/dotnet/runtime/blob/main/src/mono/wasm/features.md#multi-threading) - WASM threading support
- [Substack CORS Configuration Reference](https://substack.com/home/post/p-164745710) - CORS policy setup
