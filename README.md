# Namh.Configuration.GlobalTemplate

`Namh.Configuration.GlobalTemplate` is a design-first configuration contract
for microservices. It answers a question that service orchestration does not:
**which settings belong to the system, which belong to each service, and who
owns their values in every environment?**

For the motivation, market context, and problems this approach addresses, see
[Why GlobalTemplate?](WhyGlobalTemplate.md).

The model is deliberately simple:

- The software architect defines both `GlobalSettings.json` and each service's
  `appsettings.json`.
- `GlobalSettings.json` holds shared system values and cross-service data.
- Each service's `appsettings.json` is its application contract: its keys are
  stable for the service, and their values reference `GlobalSettings.json`.
- CI/CD supplies environment-specific values without changing that structure.
- Developers consume the service contract without coupling their code to the
  system-wide `GlobalSettings.json` graph. They can still add configuration
  required by their implementation, such as settings for a third-party
  library.

## Configuration architecture, not orchestration

GlobalTemplate and Aspire solve different problems:

| | GlobalTemplate | Aspire |
| --- | --- | --- |
| Primary question | What is the configuration contract and ownership boundary? | How do services run together during development? |
| Design approach | Design-first: explicit JSON, visible references, reviewable boundaries. | Code-first: orchestration is expressed in the developer host. |
| Environment scope | One configuration model for development, test, staging, and production. | Primarily a developer experience; production requires a separate deployment/configuration path. |
| Ownership | Architect defines both configuration files; CI/CD manages values; developers consume the service contract. | The developer host often becomes the place where service relationships and settings are assembled. |
| Change visibility | Configuration dependencies are diffable, searchable, and easy to audit. | Generated or runtime wiring can make the effective configuration harder to inspect and customize. |

This is not a replacement for Aspire's orchestration capabilities. Aspire is
excellent for composing a local distributed application and showing how
services connect. GlobalTemplate governs the configuration architecture that
must survive beyond the local developer machine: boundaries, references,
ownership, environment promotion, and operational control.

Use GlobalTemplate when the configuration itself is an architectural artifact.
Use Aspire when local service orchestration is the goal. They can be used
together, but Aspire should not be the only place where the production
configuration contract exists.

## Clear ownership model

| Role | Owns |
| --- | --- |
| **Software architect** | Both `GlobalSettings.json` and every service's `appsettings.json` contract, including boundaries, naming, structure, and allowed references. |
| **CI/CD and operations** | Environment values, secrets, validation, promotion, and controlled overrides. |
| **Developer** | Service implementation and contract consumption. Developers may add implementation-specific settings, such as third-party library configuration, without reaching into unrelated services. |

## NuGet packages

`Namh.Configuration.GlobalTemplate` is the host-integration package built on
top of [`Namh.Configuration.Template`](https://www.nuget.org/packages/Namh.Configuration.Template/).
The two packages have distinct responsibilities:

- **`Namh.Configuration.Template`** provides the configuration-template engine
  that resolves references such as `{IdProvider:Url}` and
  `{WebApi:ClientId}`.
- **`Namh.Configuration.GlobalTemplate`** integrates that engine with
  `IHostBuilder` and `IHostApplicationBuilder` through
  `ApplyTemplateConfiguration`.

`Namh.Configuration.GlobalTemplate` declares `Namh.Configuration.Template` as a
dependency, so applications normally install `GlobalTemplate` and receive the
template engine transitively:

The package produced by this repository is:

```text
Namh.Configuration.GlobalTemplate
```

Add it to an application that uses the generic host or
`WebApplicationBuilder`:

```xml
<PackageReference Include="Namh.Configuration.GlobalTemplate" Version="1.0.0" />
```

> Use the version published by your package feed. The project currently
> targets .NET 8, 9, 10 and depends on `Namh.Configuration.Template` version
> `1.0.1` for resolving configuration references.

The package relationship in the library project is explicit:

```xml
<ItemGroup>
  <PackageReference Include="Namh.Configuration.Template" Version="1.0.1" />
</ItemGroup>
```

Do not replace `Namh.Configuration.Template` with ad-hoc placeholder
replacement in the application. The template package is the component that
interprets the reference syntax and produces the resolved configuration used by
`GlobalTemplate`.

## Quick start

Call `ApplyTemplateConfiguration` while creating the host. Register
`GlobalSettings.json` before `appsettings.json` so the application file can
reference shared values and override them when the service contract permits it.

### ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args)
    .ApplyTemplateConfiguration(config =>
    {
        config.AddJsonFile("GlobalSettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    });

var app = builder.Build();

app.MapGet("/", (IConfiguration configuration) =>
    configuration["Authentication:Authority"]);

app.Run();
```

### Generic host and console applications

```csharp
IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .ApplyTemplateConfiguration(config =>
    {
        config.AddJsonFile("GlobalSettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    });

using IHost app = builder.Build();
await app.RunAsync();
```

The package provides overloads for both `IHostApplicationBuilder` (used by
`WebApplication.CreateBuilder`) and `IHostBuilder` (used by
`Host.CreateDefaultBuilder`). The resulting template configuration is added to
the host configuration and registered as `IConfiguration`.

## Configuration design

### Shared system contract

The demo's [`GlobalSettings.json`](src/Demo/GlobalSettings.json) contains the
system-level values and credentials used by more than one service:

```json
{
  "IdProvider": {
    "Url": "http://localhost:8081"
  },
  "ConsoleApp": {
    "ClientId": "consoleapp",
    "ClientSecret": "secret",
    "Scopes": [ "openid", "profile" ]
  },
  "WebApi": {
    "Url": "http://localhost:8083",
    "ClientId": "webapi",
    "ClientSecret": "secret",
    "Scopes": [ "openid", "profile", "api" ]
  }
}
```

### Architect-defined application contract

The software architect defines an application's
[`appsettings.json`](src/Demo/WebApi/appsettings.json) as its configuration
contract. The contract presents the shape the service consumes; values in
braces resolve to shared configuration:

```json
{
  "Authentication": {
    "Authority": "{IdProvider:Url}",
    "ClientId": "{WebApi:ClientId}",
    "ClientSecret": "{WebApi:ClientSecret}",
    "Scopes": "{WebApi:Scopes}"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "{WebApi:Url}"
      }
    }
  }
}
```

This makes the dependency visible in a reviewable JSON document. For example,
the Web API depends on the identity provider URL and its own client
credentials, but its code does not need to know how another service stores its
internal settings. Developers can extend this file with service-local
configuration for implementation needs, including third-party libraries, while
the architect-owned contract remains the boundary for shared settings.

### Configuration precedence

The recommended source order is:

1. `GlobalSettings.json` — shared design and environment values.
2. `appsettings.json` — service-specific shape, references, and permitted
   overrides.
3. Environment variables — deployment-time values and secrets.

Use `optional: false` for files that are mandatory in a deployed service. The
identity-provider demo does this in
[`Program.cs`](src/Demo/IdProvider/Program.cs), while the other demos use
optional files for local experimentation.

## Environment management

Keep the configuration structure stable between development, test, staging, and
production. Promote the same `GlobalSettings.json` shape and change only the
environment-specific values through CI/CD or environment variables. Secrets
should be supplied by the deployment platform's secret store rather than
committed to the repository.

The shared [`Directory.Build.targets`](src/Demo/Directory.Build.targets)
copies `GlobalSettings.json` into every demo project's build output directory.
No project needs to add its own copy rule. A production pipeline can replace
that file or inject equivalent environment variables without changing
application code or the service configuration contract.

## Repository layout

```text
src/
├── Namh.Configuration.GlobalTemplate/  # NuGet package implementation
└── Demo/
    ├── GlobalSettings.json              # shared system configuration
    ├── ConsoleApp/                      # generic-host example
    ├── IdProvider/                      # ASP.NET Core example
    └── WebApi/                          # ASP.NET Core example
```

Build the solution from the repository root:

```powershell
dotnet build .\src\GlobalConfig.slnx
```

### How `Directory.Build.targets` shares the global settings file

MSBuild automatically imports a `Directory.Build.targets` file into projects
under its directory. The demo keeps one shared target at
`src/Demo/Directory.Build.targets`, so all three demo applications copy the
same `src/Demo/GlobalSettings.json` into their build output folders:

```xml
<Project>
  <Target Name="CopyGlobalSettings" AfterTargets="Build">
    <Copy SourceFiles="$(MSBuildProjectDirectory)\..\GlobalSettings.json"
          DestinationFolder="$(OutputPath)"
          SkipUnchangedFiles="true" />
  </Target>
</Project>
```

This target is the build-time bridge between the shared configuration template
and each application's output directory. Keep `Directory.Build.targets` and
`GlobalSettings.json` at the `src/Demo` level so every demo project inherits
the rule. The target runs automatically when building the solution:

```powershell
dotnet build .\src\GlobalConfig.slnx
```

Run an individual demo with:

```powershell
dotnet run --project .\src\Demo\WebApi\WebApi.csproj
dotnet run --project .\src\Demo\IdProvider\IdProvider.csproj
dotnet run --project .\src\Demo\ConsoleApp\ConsoleApp.csproj
```
