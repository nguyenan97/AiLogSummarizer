# Use SDK to build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY OopsAI.sln ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/
COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY tools/AppHost/AppHost.csproj tools/AppHost/
COPY tools/MigrationService/MigrationService.csproj tools/MigrationService/
COPY tools/FakeDatadogApi/FakeDatadogApi.csproj tools/FakeDatadogApi/
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Restore dependencies
RUN dotnet restore OopsAI.sln

# Copy source code
COPY . .

# Build and publish WebApi
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application
COPY --from=build /app/publish ./

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebApi.dll"]