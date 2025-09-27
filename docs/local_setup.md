# Local Setup

## Prerequisites

- .NET 9.0 SDK
- Git
- (Optional) Visual Studio 2022 or VS Code with C# extension

## Clone the Repository

```bash
git clone <repository-url>
cd OopsAI
```

## Restore Packages

```bash
dotnet restore
```

## Build the Solution

```bash
dotnet build
```

## Run the Application

### Web API
```bash
dotnet run --project src/WebApi/WebApi.csproj
```

### Using Aspire App Host
```bash
dotnet run --project tools/AppHost/AppHost.csproj
```

## Database Migrations

If using Entity Framework Core:

```bash
dotnet run --project tools/MigrationService/MigrationService.csproj
```

## Testing

Run tests (when added):

```bash
dotnet test
```

## Development

- Use the provided .editorconfig for consistent code style.
- Follow the Clean Architecture principles.
- Ensure all layers respect dependency directions.