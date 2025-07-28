// Http constants providing namespaces
global using System.Net.Mime;
global using System.Security.Claims;
global using System.Text;

// Project namespaces
global using DevTKSS.MyManufacturerERP.DataContracts;
global using DevTKSS.MyManufacturerERP.WebApi.Endpoints.Todo;
global using DevTKSS.MyManufacturerERP.WebApi.Server;

// AspNetCore namespaces
global using Microsoft.AspNetCore;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Http.HttpResults;

// Database namespaces
global using Microsoft.EntityFrameworkCore;

// Identity namespaces
global using OpenIddict.Abstractions;
global using OpenIddict.Server.AspNetCore;
global using OpenIddict.Validation.AspNetCore;
global using static OpenIddict.Abstractions.OpenIddictConstants;

// OpenAPI namespaces
global using Scalar.AspNetCore;

// Logging namespaces
global using Serilog;
global using Serilog.Templates;
global using Serilog.Templates.Themes;
global using ILogger = Serilog.ILogger;
