# Copilot instructions (DevTKSS.MyManufacturerERP)

## Repo orientation
- Repository root: `C:\Users\Sonja\source\repos\DevTKSS.MyManufacturerERP`
- Solution file: `DevTKSS.MyManufacturerERP.slnx` in repo root.
- Code lives under `src/`.
- Documentation lives under `docs/`.

### Uno Platform specifics
- The Uno client app is `src/Client/DevTKSS.MyManufacturerERP/DevTKSS.MyManufacturerERP.csproj` (`<Project Sdk="Uno.Sdk">` + `<UnoSingleProject>true</UnoSingleProject>`).
- `src/Client/DevTKSS.MyManufacturerERP.WasmServer/DevTKSS.MyManufacturerERP.WasmServer.json` exists **only** to host the Uno `net10.0-browserwasm` target (it is **not** the primary Web API / Server!).
- When asked to change “the Web API” or "the Server", this means, you are requested to work on `src/WebApi/WebApi/WebApi.csproj`.
- **Do not add `sdk.version` to `global.json`** unless it is clearly required and verified for the requested automation scenario, as `Uno.Sdk` is the primary SDK.

### Solution organization specifics
- If you add CI or other repo-management files such as `.github/dependabot.yml`, you **MUST** also add them to the `/.github/` folder inside `DevTKSS.MyManufacturerERP.slnx` so they appear in the Visual Studio solution tree.
- If you create a new project, you **MUST** add it to the most suitable existing solution folder and keep path/category conventions aligned with the current structure.
- If you add an extension/library project, prefer placing it under `src/Extensions/` and group it with the appropriate extension category in the solution.
- If you are asked to add tests, prefer existing test projects under `src/Tests/` and do not duplicate existing test coverage.
  - Use `src/Tests/DevTKSS.MyManufacturerERP.UITests/DevTKSS.MyManufacturerERP.UITests.csproj` for UI tests related to `src/Client/DevTKSS.MyManufacturerERP/DevTKSS.MyManufacturerERP.csproj` or `src/Client/DevTKSS.MyManufacturerERP.WasmServer/DevTKSS.MyManufacturerERP.Server.csproj`.
  - Prefer `src/Tests/DevTKSS.MyManufacturerERP.xUnitTests/DevTKSS.MyManufacturerERP.xUnitTests.csproj` for non-UI tests and for tests covering projects under `src/Extensions/`.
- If asked to add a new Uno project (`unoapp`, `unolib`, or another `uno*` template), you **MUST** first verify that the Uno MCP tools are available. If they are unavailable and you cannot restore access to them, stop and report that the task is not currently possible instead of guessing the generated structure.

## Coding/workflow rules

### Your general workflow
- ***Always* Start your execution with:**
  1. Do a quick repo scan *before* editing and search for usages, especially for referenced files, Symbols, and types!
    - Some Types might be coming from external Packages or UnoFeatures imported implicit usings. If you can not find them in the solution, use the Uno Docs + MS Docs MCP tools to identify their origin and get future informations about them. If the Type is *not* issued by the compiler as missing, you **MUST NOT** create a new Type with the same name!
  2. Follow existing patterns and styles in the codebase.
  3. Write clear, concise, and well-documented code.
    - **"Well-documented" means specifically:**
      - You will ensure *especially public* API has XML docs, internal or private API *can* get XML docs too, evaluate this when the contained code is complex or frequently used.
      - Check existing XML docs to be up-to-date *and* usage of `<see cref="..." />` Tags with Type/Symbol referenced instead of writing the Identifier as pure string.
      - Add inline comments for complex logic along MS Docs/learning Paths for C# commenting in Code guidelines.
      - You **MUST AVOID** creating comments that don't add value or clutter the code. If the code is self-explanatory, comments are unnecessary.
      - **Targeted XML Docs purpose:**
        1. Future consumers if Packaged/Published API.
        2. DocFx Generated external documentation.
        3. Aid the Developer at dev-time, by providing intellisense/intelli-code support and avoidance of constant lookup.
      - ***When* to take care of XML Documentation:** You touched/created the type or a contained method/function.
  4. Wherever you changed/moved code/Types/Interfaces etc. you **MUST** ensure usages are updated across the solution and especially its Implementations!

### General C# Coding Guidelines
- Follow existing code patterns and styles.
- Prefer minimal diffs and reuse existing types.
- Keep C# 14/.NET 10 conventions.
- We use `GlobalUsings.cs` files for common usings in each project, avoid file-level usings unless it's really the only one using that using.
- Ensure proper nullability annotations.
- Validate with `dotnet build` (and `dotnet test` when relevant).
- Prefer using existing types and methods over creating new ones.
- Do not mix MVVM patterns into MVUX models.
- EventHandlers are only allowed in Codebehind! NOT in MVUX Models!
- You **MUST NOT** register MVUX Models as services. Using `Uno.Extensions.Navigation` and its `RegisterRoutes` in the `App.xaml.cs` will automatically register the MVUX Models for you.
- Avoid 'Try*' methods with multiple out parameters; only existing patterns like `IDictionary.TryGet*(...)` are allowed to use this! You will **ONLY** do this when explicitly requested!
- If an Interface defines a Task or ValueTask returning Method, you **MUST** implement it as async Method in the Implementing Class, unless explicitly requested otherwise!
- Do **NOT** use `Task.FromResult(...)` or `ValueTask.FromResult(...)` in Implementations of Interface Methods! The interface cannot tell you if the Implementation will be async or not.

### API Visibility Modifiers
- Avoid expanding public API surface unnecessarily. This means:
  - Do not make types/methods `public` just to consume them from another project.
  - This also applies to helper methods! Keep them private unless needed externally.
  - If a property or method is needed only within the assembly or exposing potentially security-related values, prefer `internal` visibility.
  - Prefer explicit `[assembly: InternalsVisibleTo("Exact.Assembly.Name")]` entries (no wildcards) when cross-project access to `internal` surface is required.

### Handling of Method Arguments/Parameters
- If parameters are non-nullable, do not add throwing guards/exceptions; prefer non-throwing flow consistent. Prefer argument validation via logging + early return.
- Avoid adding exception-throwing guards in MVUX flows; prefer non-throwing `Option<T>.SomeOrDefault(...)` patterns.
- In case you got a typed argument, which *cannot* simply be provided to following Methods as argument or its provided Values need to be used as part of a MVUX State `.ForEach(...)`-Callback-Method:
  - Apply the following Instructions:
    - You **MUST NOT** duplicate Property-Values of already existing strongly Typed arguments/parameters into separate backing fields!
    - You **MUST** prefer storing this existing Typed-Argument in *one* single backing field.
    - You **MUST** keep *only one* single source of truth.
    - **Example from this Codebase:**
      1. The `OAuthTokenClient.GetAuthorizeStateAsync` was used to get an `AuthorizationState`-Object.
      2. This Object now gets provided to the `IOAuthNavigationService.NavigateAuthenticationAsync(` as Method-Parameter -> in this Service the object doesn't need to be used further than transferring it. Here you **WILL NOT** store it or its Properties into backing fields.
      3. The `IOAuthNavigationService` uses to provide it to the `navigator.NavigateDataForResultAsync<AuthorizationState,WebAuthenticationResult>(sender, state, qualifier: qualifier, cancellation: cancellationToken);`-Method.
      4. Private helper Classes need this Object.
      - **Preferred Approach:** If the helper Method is *directly* called by your Method, simply provide it as an argument for the called Method instead of creating separate fields when an existing context object (e.g., with its Properties `State` / `CodeVerifier`) already carries them.

### References to external Examples/Samples
- You **MUST NOT** guess Implementations/Samples if you failed to find them!
- Use clear language and wording, so the Developer understands the context and will be able to possibly help you if you got stuck at some point.
- **Example:** The Developer found a sample that you are requested to replicate in the codebase, but you failed/had problems accessing/parsing the referenced external Sample. STOP your work and explicitly state so instead of guessing an implementation!
- Considerable Solutions you could suggest:
  - (The Sample is on GitHub): I failed to read the referenced Sample because I got Error XYZ while accessing it. Am I allowed to create a git ignored `./samples/` folder to get a local clone to inspect it?
  - (The Sample is in MS Docs): I failed to read the referenced Sample because the provided link is broken/outdated. Can you copy the relevant code snippet here so I can inspect it?
  - (The Sample is in Uno Docs): I failed to read the referenced Sample because the `uno_platform_docs_search` `uno_platform_docs_fetch` MCP tools did not return any (request if there are other keywords you missed to consider)/incomplete results (request a regular Web Url).
  - (The Uno/UnoApp MCP Server you tried to use/call has not been able to start): I did check the Uno MCP Server and UnoApp MCP Server to be started correctly, but it seems like they are not started/accessible. Here are the details I was able to gather: [insert details]. Do we want to create an issue on the Uno Platform Studio GitHub repo for this? (repo: `unoplatform/Studio`)

### Asynchronous Programming Guidelines
- Prefer `async`/`await` over synchronous blocking calls.
- Do not add `ConfigureAwait(false)` or `Task(...).Result` in this codebase unless explicitly requested!
- Prefer `async` suffix for asynchronous methods instead of `Task.FromResult`, except when this must be called by a constructor or Event handler Method.

### Auto-generated code
- You **MUST NOT** modify auto-generated code.
- Identify Generated code files by looking for common markers:
  - `// <auto-generated/>`
  - `*.g.cs`
  - Kiota CLI generated API/Client Code in `src/Client/DevTKSS.MyManufacturerERP/Clients/*`
  - Mapster.Tool generated Mappings/Mappers find `<MapsterPath>$(ProjectDir)**\mapster\**\*.g.cs</MapsterPath>`.

### PowerShell / Terminal commands
- Avoid running long recursive terminal commands (e.g., `Get-ChildItem -Recurse`)! Instead, you **MUST** use workspace indexing MCP tools (`file_search`, `code_search`) and targeted reads to prevent hangs/loops.

### Extension Helpers Guidelines
- In `DevTKSS.Extensions.OAuth.Dictionarys.IDictionaryExtensions`, follow the existing non-throwing pattern: return empty result/false on null inputs instead of throwing.
- **Keep signatures/behavior consistent with existing methods:** For bulk updates, use the existing returns of previous values (changes).
  - **Example of your Workflow:**
    - This exists: `public static string AddOrReplace(key,value)`
    - The Request you got: Implement a `AddOrReplace(IDictionary<TKey, TValue>)` or `AddOrReplaceRange(IDictionary<TKey, TValue>)` for the `.ToDictionary()`-Methods of different Request/Response Types.
    - You looked at the referenced Code and saw:
      1. The Method you are meant to work in (e.g. `OAuthProvider`) is returning `default` (=> not-throwing Behaviour) or a `IDictionary<TKey, TValue>`.
      2. In the `*Request` Types, the `string` Typed properties are either way `required`, so you don't need to care about whether they got a value -> the Initialization of the Object would already fail and your to-be-created code will never be reached.
      3. The `*Response` Types have been already validated by pattern matching before the mapping method you will write is getting called. -> not your reliability either to have excessive validation in the to-be-created Method.
    - For a single-`KeyValuePair<string,string>` (or `AddOrReplace(key,value)`) we are already having an expected working method.
    - Your assumption now is:
      - Let me check the existing method...
      - Ah, the XML docs and code show me that it returns the old Value in case of Replace or null when it added something. Let me reuse this.
      - To allow the caller to check the updated Values and even when I don't actually need this, let me keep this consistent with existing methods.
      - I will loop through the dictionary with a `foreach (var (key, value) in source)` which provides me the key and I will just request the existing `AddOrReplace(string,string)` multiple times for its job.
      - For this, I will need to create a `Dictionary<string, string>` before starting the foreach, so let's add this line right before.
      - If it then returns a value, I will simply add it by the key value my loop has at this time and I will return the created dictionary at the end of my method.

### OAuth Navigation Guidelines
- Prefer the consolidated constants provider `DevTKSS.Extensions.OAuth.Defaults.OAuthDefaults`.
- Use `IAuthenticationNavigator` for Interactive OAuth2 Authentication Flow (in preferred order):
  1. `OAuthNavigationQualifiers.SystemBrowser` ("SystemBrowser"): `ISystemBrowserAuthBrokerProvider` for usage of the system-browser (Desktop/Windows);
  2. `Qualifiers.Dialog` (provided from Uno Extensions Navigation: "!"): WebView2-based auth UI in a `ContentDialog`.
  3. `OAuthNavigationQualifiers.Window` ("Window"): WebView2-based auth UI in a new `Window`.
- Prefer exposing a single high-level method (e.g., `MyHttpClient.LoginAsync`) orchestrating private helpers, mirroring sample `OidcAuthenticationProvider` style.

## Readability preferences
- Prefer readability over micro-optimizations: avoid `static` lambda modifiers unless the codebase already uses them.
