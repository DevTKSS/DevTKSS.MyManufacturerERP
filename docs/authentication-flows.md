# Authentication Flows

This document describes the three OAuth2 authentication flows implemented for connecting to Etsy, plus the SevDesk API-key integration.

## Overview

| Flow | Platform | Mechanism | Status |
|------|----------|-----------|--------|
| WebAPI Proxy | All | Client -> WebAPI -> Etsy | In Progress |
| SystemBrowser | Desktop | Loopback HTTP server + system browser | In Progress |
| WebView2 | Desktop (Windows) | Embedded WebView2 in dialog/page | In Progress |
| SevDesk API-Key | All | Static API token in Authorization header | In Progress |

## Flow 1: WebAPI Proxy (Client -> WebAPI -> Etsy)

The server acts as an OAuth proxy. The client initiates login, the server handles the Etsy OAuth dance, and returns tokens to the client.

```
Client                          WebAPI                          Etsy
  |                               |                               |
  |-- GET /auth/login ----------->|                               |
  |                               |-- Challenge("Etsy") --------->|
  |                               |                               |
  |                               |<-- redirect with code --------|
  |                               |                               |
  |                               |-- exchange code for tokens -->|
  |                               |<-- tokens --------------------|
  |                               |                               |
  |<-- Cookie + redirect ---------|                               |
  |                               |                               |
  |-- POST /auth/token ---------->|                               |
  |<-- Bearer tokens -------------|                               |
```

### Key files

- WebAPI endpoints: `src/WebApi/WebApi/Endpoints/Authentication/OAuthEndpoints.cs`
- Client auth handler: `src/Client/DevTKSS.MyManufacturerERP/App.xaml.cs` (`HandleLoginCallbackAsync`)
- Token client: `src/Extensions/OAuth.UI/Http/OAuthTokenClient.cs`

### Configuration

WebAPI `appsettings.json`:
```json
"Authentication": {
  "Etsy": {
    "ClientId": "YOUR_ETSY_CLIENT_ID",
    "ClientSecret": "YOUR_ETSY_KEYSTRING",
    "Scope": "shops_r email_r",
    "CallbackPath": "/callback/etsy",
    "UsePkce": true
  }
}
```

Client `appsettings.development.json` needs the `OAuthClient` section with endpoints and credentials.

> Use `dotnet user-secrets` to store `ClientId` and `ClientSecret` locally.

## Flow 2: SystemBrowser with Loopback (Desktop)

The desktop client opens the system browser for Etsy login and listens for the callback on a local HTTP server (via Yllibed.HttpServer).

```
Client                     SystemBrowser              Etsy
  |                              |                      |
  |-- start loopback server ---->|                      |
  |-- open browser ------------->|                      |
  |                              |-- user authenticates |
  |                              |                      |
  |<-- callback on localhost ----|<-- redirect ---------|
  |                              |                      |
  |-- exchange code for tokens -------------------------------->|
  |<-- tokens --------------------------------------------------|
```

### Key files

- System browser broker: `src/Extensions/OAuth.UI/SystemBrowser/SystemBrowserAuthBroker.Desktop.cs`
- Browser opener: `src/Extensions/OAuth/Providers/BrowserProvider.cs`
- OAuth provider: `src/Extensions/OAuth.UI/OAuthProvider.cs`

### Known limitations

- Loopback server only supports HTTP (not HTTPS)
- Blocked by Uno Platform PR #2890 for native `IWebAuthenticationBrokerProvider` integration
- See also: [Uno Extensions PR #3019](https://github.com/nicknow/uno.extensions/pull/3019) for desktop authentication improvements
- Tracked in [EPIC #6](https://github.com/nicknow/uno.extensions/issues/6)

## Flow 3: WebView2 Embedded (Desktop/Windows)

Uses an embedded WebView2 control in a dialog or page to handle the OAuth flow without leaving the app.

```
Client                     WebView2 Dialog            Etsy
  |                              |                      |
  |-- navigate to start URL ---->|                      |
  |                              |-- load Etsy login -->|
  |                              |<-- user logs in -----|
  |                              |                      |
  |<-- detect redirect URI ------|<-- redirect ---------|
  |                              |                      |
  |-- exchange code for tokens -------------------------------->|
  |<-- tokens --------------------------------------------------|
```

### Key files

- WebView2 model: `src/Extensions/OAuth.UI.WebView2/Views/WebView2AuthenticationModel.cs`
- WebView2 page: `src/Extensions/OAuth.UI.WebView2/Views/WebView2AuthenticationPage.xaml`
- Auth dialog: `src/Client/DevTKSS.MyManufacturerERP/Presentation/Dialogs/AuthDialogModel.cs`
- Navigation service: `src/Extensions/OAuth.UI/Navigation/OAuthNavigationService.cs`

### Known limitations

- WebView2 is provided cross-platform by Uno Platform SDK 6+ (Windows, Linux, macOS via WebKitGTK/WebKit); it is not Windows-only

## SevDesk API-Key Integration

SevDesk uses a static API token (not OAuth). The token is passed as the `Authorization` header value.

### Key files

- Handler: `src/Client/DevTKSS.MyManufacturerERP/Infrastructure/SevDesk/SevDeskApiKeyHandler.cs`
- Options: `src/Client/DevTKSS.MyManufacturerERP/Infrastructure/SevDesk/SevDeskClientOptions.cs`
- Endpoints: `src/Client/DevTKSS.MyManufacturerERP/Infrastructure/SevDesk/ISevDeskEndpoints.cs`

### Configuration

```json
"SevdeskApiKeyClient": {
  "UseNativeHandler": true,
  "Url": "https://my.sevdesk.de/api/v1",
  "ApiKey": "your-api-key-from-user-secrets"
}
```

> **Deadline: 31.03.2026** - SevDesk API token authentication must be completed by this date.

## References

- [Uno Platform Getting Started](https://platform.uno/docs/articles/getting-started.html)
- [Uno Extensions Authentication](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-Authentication.html)
- [Uno Extensions Navigation](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Navigation/HowTo-Navigation.html)
- [EPIC #6 - Desktop Authentication](https://github.com/nicknow/uno.extensions/issues/6)
- [Uno Extensions PR #3019](https://github.com/nicknow/uno.extensions/pull/3019)
