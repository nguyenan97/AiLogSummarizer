# AI Log Summarizer ("OopsAI")

## 📝 Table of Contents

- [✨ Overview](#-overview)
- [🏁 Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Setting Local Environment](#setting-local-environment)
  - [Run application/service locally](#run-applicationservice-locally)
- [Known Issues](#known-issues)
- [🚀 Deployment](#-deployment)
- [📐 Code Analyzer](#-code-analyzer)
- [🏰 Architectures and Coding Structure](#-architectures-and-coding-structure)
- [👍 Pull Request Template](#-pull-request-template)
- [⭐ Git workflow](#-git-workflow)
- [🛠️ CI/CD Pipelines](#-cicd-pipelines)

## ✨ Overview

AI Log Summarizer ("OopsAI") is an intelligent log analysis assistant integrated into Slack. When mentioned in Slack (for example, `@OopsAI`), it retrieves and analyzes logs based on user queries and returns summarized insights. It:
- Highlights key root causes (e.g., dependency errors, syntax errors, null reference exceptions, DB timeouts)
- Provides concise summaries with actionable fix suggestions
- Correlates logs across multiple services (e.g., detecting a payment error caused by an upstream auth failure)
- Supports natural-language log queries
- Sends daily or on-demand Slack reports with error summaries

The solution follows Clean Architecture and targets .NET 9.0 with:
- ASP.NET Core Web API (Swagger/OpenAPI, health checks)
- Application layer using MediatR (CQRS) and FluentValidation
- Infrastructure with Entity Framework Core (SQL Server), repositories, and unit of work
- Tools: .NET Aspire AppHost for orchestration and a migration console service

Key projects:
- `src/WebApi/WebApi.csproj` (API, Swagger, health checks)
- `src/Application/Application.csproj` (MediatR, pipeline behaviours)
- `src/Infrastructure/Infrastructure.csproj` (EF Core, DI, repositories)
- `src/Domain/Domain.csproj` (entities and core abstractions)
- `tools/AppHost/AppHost.csproj` (Aspire AppHost)
- `tools/MigrationService/MigrationService.csproj` (EF Core migrations host)

Central package management and analyzer settings are configured via `Directory.Packages.props` and `Directory.Build.props`.

## 🏁 Getting Started

### Prerequisites

- [.NET SDK `9.0.100` (pinned by `global.json`)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started/) / [Podman](https://podman.io/get-started)
- [.NET Aspire CLI](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling) (optional)
- SQL Server available at `localhost` (or update the connection string)
- Git
- Optional: Visual Studio 2022 or VS Code with C# extension

### Setting Local Environment

- Configure database connection:
  - Update `src/WebApi/appsettings.json` `ConnectionStrings:DefaultConnection`, or
  - Set environment variable `ConnectionStrings__DefaultConnection="<your-connection-string>"`.
- Restore and build:
  - `dotnet restore`
  - `dotnet build`

### Run application/service locally

- Web API (Development):
  - `dotnet run --project src/WebApi/WebApi.csproj`
  - URLs: `https://localhost:7017` (`src/WebApi/Properties/launchSettings.json`)
  - Swagger UI available at `/` in Development
  - Health check at `/health`
  - Example endpoint: `GET /api/system/datetime`
- Aspire AppHost:
  - `dotnet run --project tools/AppHost/AppHost.csproj`
  - Exposes API with external HTTP endpoints (`tools/AppHost/AppHost.cs`)
- Database migration host:
  - `dotnet run --project tools/MigrationService/MigrationService.csproj`
  - Applies EF Core migrations via `Database.MigrateAsync()`

## Known Issues

- Health checks require a reachable SQL Server using the configured connection string; otherwise `/health` reports Unhealthy.
- EF Core migrations must exist to apply schema changes; if none are created yet, the migration host will be a no-op.
- Local builds treat warnings as errors (`Directory.Build.props`); the CI workflow overrides this for PR builds.

## 🚀 Deployment

- Publish the API:
  - `dotnet publish src/WebApi/WebApi.csproj -c Release -o ./publish`
- Required environment configuration:
  - `ConnectionStrings__DefaultConnection` must be set for the target environment
  - `ASPNETCORE_ENVIRONMENT` typically `Production`
- Run the published app: `dotnet ./publish/WebApi.dll`

## 📐 Code Analyzer

- Built-in Roslyn analyzers enabled with `AnalysisLevel=latest` and `EnforceCodeStyleInBuild=true` (`Directory.Build.props`).
- `TreatWarningsAsErrors=true` locally to keep code quality high (`Directory.Build.props`).
- `.editorconfig` defines detailed code style and diagnostic severities (IDE/CA rules), with nullable and implicit usings enabled solution-wide.

## 🏰 Architectures and Coding Structure

- Domain (`src/Domain`): core entities and abstractions (no dependencies).
- Application (`src/Application`): CQRS with MediatR and FluentValidation; pipeline behaviours for performance, validation, and logging (`src/Application/Common/Behaviours`).
- Infrastructure (`src/Infrastructure`): EF Core `ApplicationDbContext`, repositories, unit of work, time service; DI wiring in `src/Infrastructure/DependencyInjection.cs`.
- Web API (`src/WebApi`): controllers and composition root; Swagger, health checks, and endpoint mapping in `src/WebApi/Program.cs`.
- Tools:
  - Aspire AppHost orchestrates the API (`tools/AppHost/AppHost.cs`).
  - MigrationService applies EF migrations on startup (`tools/MigrationService/Program.cs`).
- Centralized NuGet versions via `Directory.Packages.props`.

## 👍 Pull Request Template

- PRs should follow the checklist in `docs/pull_request_template.md` covering coding principles, style, quality, architecture/design, and tests.

## ⭐ Git workflow

- Use feature branches and open Pull Requests targeting `main`.
- Automated builds validate PRs.
- Dependency updates are managed by Dependabot (`.github/dependabot.yml`).

## 🛠️ CI/CD Pipelines

- CI/CD pipelines are defined in ./github/ folder
- GitHub Actions workflow builds pull requests on `main` (`.github/workflows/build-pull-request.yml`).
  - Sets up .NET 9, restores, and builds in Release.


