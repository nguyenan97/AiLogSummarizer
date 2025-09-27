# OopsAI

A .NET 9.0 solution following Clean Architecture principles with DDD, CQRS, and Dependency Injection.

## Project Structure

- **Domain**: Core business logic and entities.
- **Application**: Application services and use cases.
- **Infrastructure**: External concerns like data access and services.
- **WebApi**: ASP.NET Core Web API.
- **AppHost**: .NET Aspire application host.
- **MigrationService**: Console app for database migrations.

## Getting Started

1. Ensure .NET 9.0 is installed.
2. Restore packages: `dotnet restore`
3. Build: `dotnet build`
4. Run: `dotnet run --project src/WebApi/WebApi.csproj`

## Architecture

This solution follows Clean Architecture with the following layers:

- Domain
- Application
- Infrastructure
- WebApi

Each layer has dependencies only on inner layers.