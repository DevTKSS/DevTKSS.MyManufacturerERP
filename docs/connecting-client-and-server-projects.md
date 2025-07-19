# Connecting Client and Server Projects

## Setting up `launchSettings.json` for Uno Platform Projects

When working with Uno Platform projects, especially those targeting WebAssembly and ASP.NET Core, you may encounter issues related to the `launchSettings.json` configuration. This document outlines common problems and their solutions or at least some notes you might should notice on the way to get to a Solution.

- Server might not have been started when clicking "Login" button in client application (desktop target)
  -> As workaround, we are required to start the server with one of those options:

    1. manually from command line with `dotnet run`
    1. use Visual Studio Build order
    1. Aspire/Docker Compose Orchestration (Aspire maybe only for WebAssembly target)

### Port configuration conflicts between WebAssembly and Server targets

To avoid this issue, it's important, to make sure the ports of our WebAssembly Project are not "trying" to listen on the same Ports as our ASP.NET Core aka "Server" Project does.

By default, the `launchSettings.json` file in a templated Uno App with WebAssembly / `net9.0-browserwasm` target, does come with these settings:

```json
{
"iisSettings": {
  "windowsAuthentication": false,
  "anonymousAuthentication": true,
  "iisExpress": {
    "applicationUrl": "http://localhost:8080",
    "sslPort": 0
  }
},
"profiles": {
  // This profile is first in order for dotnet run to pick it up by default
  "UnoApp1 (WebAssembly)": {
    "commandName": "Project",
    "dotnetRunMessages": true,
    "launchBrowser": true,
    "applicationUrl": "http://localhost:5000",
    "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  },
  "UnoApp1 (WebAssembly IIS Express)": {
    "commandName": "IISExpress",
    "launchBrowser": true,
    "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  },
// code of eventual other target platforms obmitted for brevity
```

So if you may notice, we are serving our Application at the `http` port with the number `5000` or if we are working with the `ISS` launch profile, this will be the `http` port `8080`.

Now, we comparing to the `launchSettings.json` file of our ASP.NET Core Server Project, we will find the following settings:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:5001",
      "sslPort": 5002
    }
  },
  "profiles": {
    "UnoApp1.Server": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "",
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "applicationUrl": "http://localhost:5001;https://localhost:5002",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "",
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Notice here, that the Ports are `5001` for `http` and `5002` for `https`. This means, that if we are running our Uno Platform WebAssembly Project, they are not listening on the same ports as our ASP.NET Core Server Project does. So, we are safe here.

But still, now lets say nowerdays, we need to integrate https with [Cross-Side Request Forgery (CSRF)](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-9.0) protection, which is required by the Etsy API, for example. This means, that we need to run our Uno Platform WebAssembly Project with `https` enabled. As you can see, this is not so far the case in our `launchSettings.json` file, so we need to add the `https` port to our Uno Platform WebAssembly Project.

Problem with this is, no-one is telling us, how to do this, so we need to figure it out by ourselves, while missing any documentation about this.

### Issues may occur when setting the ports up conflicting

- Web Socket Error in ws_wasm_create most likely caused by not proper launchsettings.json configuration and both projects are trying to use the same port:

  ```plaintext
  Verbose logs are written to:
  C:\Users\Sonja\AppData\Local\Temp\visualstudio-js-debugger.txt
  Das Programm "service-worker.js" wurde mit Code 4294967295 (0xffffffff) beendet.
  Setting DOTNET_MODIFIABLE_ASSEMBLIES=debug
  Setting UNO_BOOTSTRAP_MONO_RUNTIME_MODE=Interpreter
  Setting UNO_BOOTSTRAP_MONO_PROFILED_AOT=False
  Setting UNO_BOOTSTRAP_LINKER_ENABLED=False
  Setting UNO_BOOTSTRAP_DEBUGGER_ENABLED=True
  Setting UNO_BOOTSTRAP_MONO_RUNTIME_CONFIGURATION=Release
  Setting UNO_BOOTSTRAP_MONO_RUNTIME_FEATURES=
  Setting UNO_BOOTSTRAP_APP_BASE=package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8
  Setting UNO_BOOTSTRAP_WEBAPP_BASE_PATH=/
  [0m[48;5;127m[38;5;231mdotnet[0m[1m Loaded 101.81 MB resources[0m
  This application was built with linking (tree shaking) disabled. 
  Published applications will be significantly smaller if you install wasm-tools workload. 
  See also https://aka.ms/dotnet-wasm-features[0m
  Loaded 101.81 MB resources from cache
  Debugging hotkey: Shift+Alt+D (when application has focus)
  Active service worker found, skipping register
  MONO_WASM: WebSocket error in ws_wasm_create: SecurityError: Failed to construct 'WebSocket': An insecure WebSocket connection may not be initiated from a page loaded over HTTPS.
  MONO_WASM: Assert failed: ERR18: expected ws instance Error: Assert failed: ERR18: expected ws instance
  MONO_WASM: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime. Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime.
  MONO_WASM: Assert failed: .NET runtime already exited with 1 Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime.. You can use runtime.runMain() which doesn't exit the runtime. Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime.. You can use runtime.runMain() which doesn't exit the runtime.
  Uncaught Error Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime.
    at Ke (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\loader\globals.ts:148:19)
    at tt (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\loader\exit.ts:18:190)
    at dr (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:484:19)
    at Cr (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\gc-handles.ts:74:5)
    at ho (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\marshal-to-cs.ts:349:31)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:244:13)
    at Fc (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:78:5)
    at $do_icall (wasm/dotnet.native.wasm-06346286.wat:533859:1)
    at $do_icall_wrapper (wasm/dotnet.native.wasm-06346286.wat:527254:1)
    at $mono_interp_exec_method (wasm/dotnet.native.wasm-06346286.wat:498990:1)
    at $interp_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:529486:1)
    at $mono_jit_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:1019976:1)
    at $do_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:913031:1)
    at $mono_runtime_try_invoke (wasm/dotnet.native.wasm-06346286.wat:914104:1)
    at $mono_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:918965:1)
    at $mono_wasm_invoke_jsexport (wasm/dotnet.native.wasm-06346286.wat:4373498:1)
    at <anonymous> (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:812:12)
    at ccall (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:10469:17)
    at <anonymous> (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:10487:27)
    at hn (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\managed-exports.ts:280:16)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\managed-exports.ts:65:9)
    at Pc (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\run.ts:71:22)
    --- await ---
    at mainInit (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:267:30)
    --- await ---
    at checkDone (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:369:38)
    at processDependency (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:387:37)
    at execCb (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16727)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:10499)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:12915)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1542)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13376)
    at each (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1020)
    at emit (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13344)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:11058)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13242)
    at init (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9605)
    at a (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:8305)
    at completeLoad (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16151)
    at onScriptLoad (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16882)
    --- script ---
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:3095)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:3272)
    at load (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16680)
    at load (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:10087)
    at fetch (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9888)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:11152)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13242)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15790)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13099)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1542)
    at each (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1020)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:12599)
    at init (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9605)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15109)
    --- setTimeout ---
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:2434)
    at s (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15048)
    at requirejs (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:2335)
    at require (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:412:25)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:390:34)
    at initializeRequire (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:372:58)
    at RuntimeReady (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:213:26)
    at Bootstrapper.onDotnetReady (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:124:53)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\startup.ts:451:30)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\startup.ts:351:15)
  Uncaught Error Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: .NET runtime already exited with 1 Error: Assert failed: ERR18: expected ws instance. You can use runtime.runMain() which doesn't exit the runtime.. You can use runtime.runMain() which doesn't exit the runtime.
    at Ke (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\loader\globals.ts:148:19)
    at tt (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\loader\exit.ts:18:190)
    at dr (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:484:19)
    at yr (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-cs.ts:376:5)
    at initializeExports (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/Uno.Wasm.js:65:57)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/Uno.Wasm.js:52:47)
    at run (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/setImmediate.js:46:17)
    at runIfPresent (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/setImmediate.js:75:21)
    at onGlobalMessage (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/setImmediate.js:115:17)
    --- postMessage ---
    at registerImmediate (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/setImmediate.js:126:20)
    at setImmediate (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/setImmediate.js:33:9)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/Uno.Wasm.js:51:92)
    at initialize (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/Uno.Wasm.js:51:62)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:222:13)
    at Fc (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\invoke-js.ts:78:5)
    at $do_icall (wasm/dotnet.native.wasm-06346286.wat:533859:1)
    at $do_icall_wrapper (wasm/dotnet.native.wasm-06346286.wat:527254:1)
    at $mono_interp_exec_method (wasm/dotnet.native.wasm-06346286.wat:498990:1)
    at $interp_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:529486:1)
    at $mono_jit_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:1019976:1)
    at $do_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:913031:1)
    at $mono_runtime_try_invoke (wasm/dotnet.native.wasm-06346286.wat:914104:1)
    at $mono_runtime_class_init_full (wasm/dotnet.native.wasm-06346286.wat:913467:1)
    at $mono_interp_transform_method (wasm/dotnet.native.wasm-06346286.wat:566338:1)
    at $do_transform_method (wasm/dotnet.native.wasm-06346286.wat:526045:1)
    at $mono_interp_exec_method (wasm/dotnet.native.wasm-06346286.wat:525704:1)
    at $interp_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:529486:1)
    at $mono_jit_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:1019976:1)
    at $do_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:913031:1)
    at $mono_runtime_try_invoke (wasm/dotnet.native.wasm-06346286.wat:914104:1)
    at $mono_runtime_invoke (wasm/dotnet.native.wasm-06346286.wat:918965:1)
    at $mono_wasm_invoke_jsexport (wasm/dotnet.native.wasm-06346286.wat:4373498:1)
    at <anonymous> (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:812:12)
    at ccall (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:10469:17)
    at <anonymous> (localhost꞉5001/_framework/dotnet.native.vh2bwj4kkt.js:10487:27)
    at hn (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\managed-exports.ts:280:16)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\managed-exports.ts:65:9)
    at Pc (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\run.ts:71:22)
    --- await ---
    at mainInit (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:267:30)
    --- await ---
    at checkDone (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:369:38)
    at processDependency (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:387:37)
    at execCb (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16727)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:10499)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:12915)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1542)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13376)
    at each (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1020)
    at emit (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13344)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:11058)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13242)
    at init (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9605)
    at a (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:8305)
    at completeLoad (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16151)
    at onScriptLoad (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16882)
    --- script ---
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:3095)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:3272)
    at load (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:16680)
    at load (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:10087)
    at fetch (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9888)
    at check (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:11152)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13242)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15790)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:13099)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1542)
    at each (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:1020)
    at enable (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:12599)
    at init (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:9605)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15109)
    --- setTimeout ---
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:2434)
    at s (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:15048)
    at requirejs (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/require.js:5:2335)
    at require (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:412:25)
    at <anonymous> (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:390:34)
    at initializeRequire (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:372:58)
    at RuntimeReady (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:213:26)
    at Bootstrapper.onDotnetReady (localhost꞉5001/package_21b5f74264dfbde23ef8f426f23aa85db3d9f0e8/uno-bootstrap.js:124:53)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\startup.ts:451:30)
    at <anonymous> (c:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\_framework\https:\raw.githubusercontent.com\dotnet\runtime\3c298d9f00936d651cc47d221762474e25277672\src\mono\browser\runtime\startup.ts:351:15)
  ```

To resolve the Web Socket Error in `ws_wasm_create`, you need to ensure that your `launchsettings.json` configuration is set up correctly, particularly regarding the ports used by your projects.
