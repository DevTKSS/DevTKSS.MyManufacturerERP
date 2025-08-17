global using ApplicationExecutionState = Windows.ApplicationModel.Activation.ApplicationExecutionState;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Hosting.Internal;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Security.Cryptography;
global using System.Text;
global using System.Threading.Tasks;
global using Microsoft.IdentityModel.Tokens;
global using Uno;
#if IOS || MACCATALYST
global using Apple.CryptoKit;
global using Foundation;
#endif
global using Windows.UI.ViewManagement.Core;
global using OpenIddict.Sandbox.UnoClient;