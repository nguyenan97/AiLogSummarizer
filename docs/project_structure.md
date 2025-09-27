# Project Structure

## Overview

The OopsAI solution is organized following Clean Architecture principles, with clear separation of concerns across multiple layers.

## Directory Structure

```
OopsAI/
├── src/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── WebApi/
├── tools/
│   ├── AppHost/
│   └── MigrationService/
├── docs/
├── infrastructure/
├── scripts/
├── .editorconfig
├── .gitattributes
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── OopsAI.sln
└── README.md
```

## Layer Descriptions

### Domain Layer (src/Domain)
- Contains core business entities, value objects, and domain logic.
- No dependencies on other layers.
- Represents the innermost layer.

### Application Layer (src/Application)
- Contains application services, commands, queries, and handlers.
- Depends on Domain layer.
- Implements CQRS pattern.

### Infrastructure Layer (src/Infrastructure)
- Contains external concerns: data access, external services, etc.
- Depends on Application layer.
- Implements interfaces defined in Application layer.

### WebApi Layer (src/WebApi)
- ASP.NET Core Web API project.
- Depends on Infrastructure layer.
- Handles HTTP requests and responses.

### Tools
- **AppHost**: .NET Aspire application host for orchestration.
- **MigrationService**: Console application for database migrations.

## Dependencies

The dependency flow is strictly inward:
WebApi → Infrastructure → Application → Domain

This ensures high cohesion and low coupling.