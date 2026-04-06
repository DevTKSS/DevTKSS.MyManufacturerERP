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
- **Do not add `sdk.version` to `global.json`**. `Uno.Sdk` builds upon the .NET Sdk, so you assume, its available for the .NET Version defined in Section #General-C#-Coding-Guidelines of this document.

### Solution organization specifics
- If you add CI or other repo-management files such as `.github/dependabot.yml`, you **MUST** also add them to the `/.github/` folder inside `DevTKSS.MyManufacturerERP.slnx` so they appear in the Visual Studio solution tree.
- If you create a new project, you **MUST** add it to the most suitable existing solution folder and keep path/category conventions aligned with the current structure.
- If you add an extension/library project, prefer placing it under `src/Extensions/` and group it with the appropriate extension category in the solution.
- If you are asked to add tests, prefer existing test projects under `src/Tests/` and do not duplicate existing test coverage.
  - Use `src/Tests/DevTKSS.MyManufacturerERP.UITests/DevTKSS.MyManufacturerERP.UITests.csproj` for UI tests related to `src/Client/DevTKSS.MyManufacturerERP/DevTKSS.MyManufacturerERP.csproj` or `src/Client/DevTKSS.MyManufacturerERP.WasmServer/DevTKSS.MyManufacturerERP.Server.csproj`.
  - Prefer `src/Tests/DevTKSS.MyManufacturerERP.xUnitTests/DevTKSS.MyManufacturerERP.xUnitTests.csproj` for non-UI tests and for tests covering projects under `src/Extensions/`.
- If asked to add a new Uno project (`unoapp`, `unolib`, or another `uno*` template), you **MUST** first verify that the Uno MCP tools are available. If they are unavailable and you cannot restore access to them, stop and report that the task is not currently possible instead of guessing the generated structure.

## Coding/workflow rules

### General C# Coding Guidelines
- Follow existing code patterns and styles.
- Prefer minimal diffs and reuse existing types.
- Keep C# 14/.NET 10 conventions.
- We use `GlobalUsings.cs` files for common usings in each project, avoid file-level usings unless it's really the only one using that using.
- Ensure proper nullability annotations.
- Validate with `dotnet build` (and `dotnet test` when relevant).
- Prefer using existing types and methods over creating new ones.

### Asynchronous Programming Guidelines
- Prefer `async`/`await` over synchronous blocking calls.
- Do not add `ConfigureAwait(false)` or `Task(...).Result` in this codebase unless explicitly requested!
- Prefer `async` suffix for asynchronous methods instead of `Task.FromResult`, except when this must be called by a constructor or Event handler Method.
- Do **NOT** use `Task.FromResult(...)` or `ValueTask.FromResult(...)` in Implementations of Interface Methods! The interface cannot tell you if the Implementation will be async or not.
- If an Interface defines a Task or ValueTask returning Method, you **MUST** implement it as async Method in the Implementing Class, unless explicitly requested otherwise!

### Uno Platform App Specific Strict and Important Guidelines!!
*Applying as soon as the .csproj is defining `<Project Sdk="Uno.Sdk">` with any `UnoFeatures`, but especially if they are containing `Mvux` or `Navigation`!!!*
- Do not mix MVVM patterns into MVUX models.
- EventHandlers are only allowed in Codebehind! NOT in MVUX Models!
- You **MUST NOT** register MVUX Models as services. Using `Uno.Extensions.Navigation` and its `RegisterRoutes` in the `App.xaml.cs` will automatically register the MVUX Models for you.

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

### API Visibility Modifiers
- Avoid expanding public API surface unnecessarily. This means:
  - Do not make types/methods `public` just to consume them from another project.
  - This also applies to helper methods! Keep them private unless needed externally.
  - If a property or method is needed only within the assembly or exposing potentially security-related values, prefer `internal` visibility.
  - Prefer explicit `[assembly: InternalsVisibleTo("Exact.Assembly.Name")]` entries (no wildcards) when cross-project access to `internal` surface is required.

### Handling of Method Arguments/Parameters
- If parameters are non-nullable, do not add throwing guards/exceptions; prefer non-throwing flow consistent. Prefer argument validation via logging + early return.
- Avoid adding exception-throwing guards in MVUX flows; prefer non-throwing `Option<T>.SomeOrDefault(...)` patterns.
 
### Auto-generated code
- You **MUST NOT** modify auto-generated code.
- Identify Generated code files by looking for common markers:
  - `// <auto-generated/>`
  - `*.g.cs`
  - Kiota CLI generated API/Client Code in `src/Client/DevTKSS.MyManufacturerERP/Clients/*`
  - Mapster.Tool generated Mappings/Mappers find `<MapsterPath>$(ProjectDir)**\mapster\**\*.g.cs</MapsterPath>`.
