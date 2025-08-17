global using System.Net.Mime;
global using System.Text;
global using System.Text.Json.Serialization;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;
global using DevTKSS.MyManufacturerERP.Infrastructure.Serialization;
global using DevTKSS.MyManufacturerERP.Models;
global using DevTKSS.MyManufacturerERP.Presentation;
global using DevTKSS.MyManufacturerERP.Services.Endpoints;
global using DevTKSS.MyManufacturerERP.Infrastructure.Defaults;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using DevTKSS.MyManufacturerERP.Extensions;
global using DevTKSS.MyManufacturerERP.Infrastructure;
global using DevTKSS.MyManufacturerERP.Infrastructure.Services;
global using Serilog;
global using FluentValidation;
global using ValidationResult = FluentValidation.Results.ValidationResult;
global using DevTKSS.MyManufacturerERP.Validation;
global using ValidationException = FluentValidation.ValidationException;
global using ILogger = Serilog.ILogger;
#if !WINDOWS
#endif
global using Refit;
[assembly: Uno.Extensions.Reactive.Config.BindableGenerationTool(3)]


