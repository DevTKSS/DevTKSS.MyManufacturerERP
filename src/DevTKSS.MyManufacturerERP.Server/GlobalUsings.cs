// System usings
global using System.Security.Claims;
global using System.Text.Json;
// Project usings
global using DevTKSS.MyManufacturerERP.DataContracts;
global using DevTKSS.MyManufacturerERP.Server.Apis;
global using DevTKSS.MyManufacturerERP.Server.Database;
global using DevTKSS.MyManufacturerERP.Server.Extensions;
// Http and authentication usings
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Authentication.OAuth;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Net.Http.Headers;
// OpenAPI usings
global using Scalar.AspNetCore;
global using Serilog;
// Logging usings
global using Serilog.Templates;
global using Serilog.Templates.Themes;
global using Uno.Wasm.Bootstrap.Server;
global using ILogger = Serilog.ILogger;