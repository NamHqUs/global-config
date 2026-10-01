# Why GlobalTemplate?

Configuration is not just a technical detail. It is an architectural boundary.

In microservice systems, configuration often becomes difficult to manage:

- Shared values are duplicated across services.
- Service dependencies are hidden inside code or deployment scripts.
- Development and production configuration slowly drift apart.
- Nobody is sure which team owns which setting.
- Local orchestration tools become the only place where system relationships are defined.

That is why I created Namh.Configuration.GlobalTemplate for .NET applications.

## Design-first configuration model

- `GlobalSettings.json` contains shared system-level values.
- Each service keeps its own `appsettings.json` contract.
- Services reference shared values explicitly.
- Configuration dependencies are visible in code review.
- Architects own the structure and boundaries.
- CI/CD owns environment-specific values and secrets.
- Developers consume a stable service-level contract.

For example:

```json
{
  "Authentication": {
    "Authority": "{IdProvider:Url}",
    "ClientId": "{WebApi:ClientId}"
  }
}
```

This makes the dependency between services explicit without forcing application code to understand how another service stores its internal configuration.

## Designed to complement existing tools

GlobalTemplate is not intended to replace .NET Aspire, Kubernetes, Azure App Configuration, Consul, or a secret manager.

Instead, it addresses a different question:

> What is the configuration contract of the system, who owns it, and how do services depend on it?

- Aspire can orchestrate services locally.
- Kubernetes can inject configuration into workloads.
- Azure App Configuration can manage runtime values and feature flags.
- A vault can protect secrets.

But the architecture still needs a clear, reviewable configuration contract.

That is the problem GlobalTemplate is designed to solve.

The package supports both generic hosts and ASP.NET Core applications through
`ApplyTemplateConfiguration`, with support for .NET 8, .NET 9, and .NET 10.

Project repository: [github.com/NamHqUs/global-config](https://github.com/NamHqUs/global-config)

What approach does your team use to prevent configuration drift across microservices?

#dotnet #dotnetdeveloper #microservices #softwarearchitecture #configurationmanagement #aspnetcore #devops #cloudnative #opensource
