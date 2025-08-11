global using System.ComponentModel.DataAnnotations;
global using System.Net.Mime;
global using System.Text;
global using System.Text.Json.Serialization;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints;
global using DevTKSS.MyManufacturerERP.Infrastructure.Endpoints.Responses;
global using DevTKSS.MyManufacturerERP.Infrastructure.Serialization;
global using DevTKSS.MyManufacturerERP.Models;
global using DevTKSS.MyManufacturerERP.Presentation;
global using DevTKSS.MyManufacturerERP.Services.Endpoints;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
#if !WINDOWS
#endif
global using Refit;
[assembly: Uno.Extensions.Reactive.Config.BindableGenerationTool(3)]
