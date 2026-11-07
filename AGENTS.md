# Remindly development guide

## Architecture

- `src/Remindly.Domain` contains business concepts and rules. It must remain independent of frameworks and other projects.
- `src/Remindly.Application` contains use cases and application contracts. It may depend on Domain, but not Infrastructure or API.
- `src/Remindly.Infrastructure` contains implementations for external systems and persistence. It may depend on Application and Domain.
- `src/Remindly.API` is the ASP.NET Core entry point, HTTP boundary, and composition root. It may depend on Application and Infrastructure.
- `tests/Remindly.UnitTests` contains fast automated tests, starting with Domain and Application behavior.
- `docs` holds project documentation.

Keep dependencies pointing inward. Domain and Application must not depend on infrastructure technologies, web frameworks, or provider SDKs. AI output is untrusted input: validate it through application use cases before performing business operations. AI components must not access the database directly.

## Coding conventions

- Target .NET 10 and use nullable reference types and implicit usings.
- Prefer clear names, small focused types, and asynchronous I/O APIs where appropriate.
- Keep business rules out of API endpoints and infrastructure adapters.
- Add dependencies only when a concrete requirement needs them; avoid adding persistence, job, AI, or messaging packages speculatively.
- Keep credentials out of source control. Use local secrets or environment configuration for development and deployment secret storage in hosted environments.
- Store and interpret scheduled times with explicit UTC and user-timezone semantics; do not assume the server timezone.

## Development and validation

- Restore: `dotnet restore Remindly.sln`
- Build: `dotnet build Remindly.sln --no-restore`
- Test: `dotnet test Remindly.sln --no-build`
- Run the API: `dotnet run --project src/Remindly.API`
- Keep tests deterministic and independent of live external services. Add integration tests when a real boundary (such as HTTP or persistence) is introduced.
- Review project references before adding one; preserve the dependency direction above.

## Instructions for future Codex tasks

- Read this file and inspect the relevant existing files before making changes.
- Preserve existing behavior and configuration unless the task explicitly asks to change them.
- Make focused changes; do not implement unrequested product features or add speculative abstractions.
- Do not add external service credentials to files.
- Explain material changes and validation results clearly, and run the checks requested by the user.
