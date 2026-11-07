# Remindly

Remindly is planned as an AI-powered conversational WhatsApp personal reminder assistant. This repository currently contains the initial .NET solution structure; reminder behavior and external integrations have not been implemented.

## Prerequisites

- .NET 10 SDK

## Projects

- `src/Remindly.API` — ASP.NET Core API and composition root
- `src/Remindly.Application` — application use cases and contracts
- `src/Remindly.Domain` — business model and rules
- `src/Remindly.Infrastructure` — external system implementations
- `tests/Remindly.UnitTests` — xUnit tests
- `docs` — project documentation

## Build and run

```powershell
dotnet restore Remindly.sln
dotnet build Remindly.sln
dotnet test Remindly.sln
dotnet run --project src/Remindly.API
```

See [AGENTS.md](AGENTS.md) for architecture boundaries and development guidance.
