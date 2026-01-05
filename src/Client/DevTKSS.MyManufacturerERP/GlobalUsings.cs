global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using DevTKSS.Extensions.OAuth.Defaults;
global using DevTKSS.Extensions.OAuth.Dictionarys;
global using DevTKSS.Extensions.OAuth.Endpoints;
global using DevTKSS.Extensions.OAuth.Responses;
global using DevTKSS.Extensions.OAuth.Requests;
global using DevTKSS.Extensions.OAuth.Validation;
global using DevTKSS.MyManufacturerERP.Infrastructure;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;
global using DevTKSS.MyManufacturerERP.Infrastructure.EtsyDefaults;
global using DevTKSS.MyManufacturerERP.Infrastructure.Services;
global using DevTKSS.MyManufacturerERP.Models;
global using DevTKSS.MyManufacturerERP.Presentation;
global using DevTKSS.MyManufacturerERP.Services.Endpoints;
global using DevTKSS.Extensions.OAuth;
global using DevTKSS.MyManufacturerERP.Presentation.Dialogs;
global using Microsoft.Extensions.Configuration;
global using Yllibed.HttpServer;
global using Yllibed.HttpServer.Handlers.Uno;
global using FluentValidation;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Refit;
global using Serilog;
global using Windows.Security.Authentication.Web;
global using ILogger = Serilog.ILogger;
[assembly: Uno.Extensions.Reactive.Config.BindableGenerationTool(3)]


