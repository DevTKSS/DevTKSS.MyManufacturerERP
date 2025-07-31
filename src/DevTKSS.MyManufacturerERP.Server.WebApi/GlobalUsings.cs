// Http constants providing namespaces
global using System.Security.Claims;
global using System.Net.Mime;
global using System.Text;
// Web slice namespaces
global using DevTKSS.MyManufacturerERP.Web.Endpoints.Todo;
global using DevTKSS.MyManufacturerERP.Web.Endpoints.Weather;
global using DevTKSS.MyManufacturerERP.Web.Endpoints.Authentication;
global using DevTKSS.MyManufacturerERP.Web.Server;
// Domain slice namespaces
global using DevTKSS.MyManufacturerERP.Domain.Entities;

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
// OpenAPI namespaces
global using Scalar.AspNetCore;
// Logging namespaces
global using Serilog;
global using Serilog.Templates;
global using Serilog.Templates.Themes;
global using static OpenIddict.Abstractions.OpenIddictConstants;
global using ILogger = Serilog.ILogger;
